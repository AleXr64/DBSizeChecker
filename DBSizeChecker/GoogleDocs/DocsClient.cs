using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DBSizeChecker.ConfigModel;
using DBSizeChecker.DB;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace DBSizeChecker.GoogleDocs
{
    /// <summary>
    ///     Perfoms updates on selected document
    /// </summary>
    public class GoogleClient
    {
        private static readonly string[] Scopes = {SheetsService.Scope.Drive, DriveService.Scope.Drive};
        private static readonly string ApplicationName = "BARS Group DBSizeChecker test app";
        private readonly DriveService _drive;
        private readonly GoogleSettings _settings;
        private readonly SheetsService _sheets;

        private string _ownSpreadsheetId;

        public GoogleClient(GoogleSettings settings)
        {
            _settings = settings;
            UserCredential credential;

            using(var stream =
                new FileStream(_settings.PathToCredentials, FileMode.Open, FileAccess.Read))
                {
                    var credPath = "token.json";

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                                                                             Scopes, "user",
                                                                             CancellationToken.None,
                                                                             new FileDataStore(credPath, true))
                                                             .Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

            // Create Google Drive API service.
            _drive = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential, ApplicationName = ApplicationName
                });
            // Create Google Sheets API service.
            _sheets = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });
        }

        /// <summary>
        ///     Check, if spreadsheet exists on drive
        /// </summary>
        /// <param name="sheet_id"></param>
        /// <returns></returns>
        private bool CheckFileExists(out string sheet_id)
        {
            var listRequest = _drive.Files.List();
            listRequest.Fields = "files(id, name)";
            var files = listRequest.Execute().Files;
            if(files != null && files.Count > 0)
                if(files.Any(x => x.Name == _settings.SheetName))
                    {
                        sheet_id = files.First(x => x.Name == _settings.SheetName).Id;
                        return true;
                    }

            sheet_id = string.Empty;
            return false;
        }

        private string CreateSpreadsheetFile()
        {
            var sheet = new Spreadsheet();
            sheet.Properties = new SpreadsheetProperties();
            sheet.Properties.Title = _settings.SheetName;
            var request = _sheets.Spreadsheets.Create(sheet);
            return request.Execute().SpreadsheetId;
        }

        private void ObtainSheetId()
        {
            if(!CheckFileExists(out _ownSpreadsheetId)) _ownSpreadsheetId = CreateSpreadsheetFile();
        }

        /// <summary>
        ///     Write info about host to spreadsheet
        /// </summary>
        /// <param name="hosts"></param>
        public void Update(List<HostInfo> hosts)
        {
            ObtainSheetId();
            foreach(var hostInfo in hosts) MakeNewData(hostInfo);
        }

        /// <summary>
        ///     Prepare data and write to sheet
        /// </summary>
        /// <param name="info"></param>
        private void MakeNewData(HostInfo info)
        {
            var freeSpace = info.TotalSpace;

            PrepareSheet(info.HostID);
            var data = new ValueRange();
            data.Range = $"{info.HostID}!A:D";

            data.Values = new List<IList<object>>();

            //header
            var head = new List<object>();
            head.AddRange(new[] {"Сервер", "База данных", "Размер в GB", "Дата обновления"});
            data.Values.Add(head);

            //Database stats records
            foreach(var infoDataBase in info.DataBases)
                {
                    freeSpace = freeSpace - infoDataBase.SizeOnDiskInGB;

                    var row = new List<object>();
                    row.Add(info.HostID);
                    row.Add(infoDataBase.DBName);
                    row.Add(infoDataBase.SizeOnDiskInGB);
                    row.Add(DateTime.Today.ToString("dd.MM.yyyy",
                                                    CultureInfo.InvariantCulture));
                    data.Values.Add(row);
                }

            data.Values.Add(new List<object>()); //spacer

            //"total space left" report
            var sizeReport = new List<object>();
            sizeReport.AddRange(new[]
                {
                    info.HostID, "Свободно", Math.Round(freeSpace, 3).ToString(),
                    DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                });

            data.Values.Add(sizeReport);

            // Add data to sheet
            var appendRequest = _sheets.Spreadsheets.Values.Append(data, _ownSpreadsheetId, data.Range);
            appendRequest.ValueInputOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = appendRequest.Execute();
        }

        /// <summary>
        ///     Re-create sheet
        /// </summary>
        /// <param name="name"></param>
        private void PrepareSheet(string name)
        {
            var spread = _sheets.Spreadsheets.Get(_ownSpreadsheetId).Execute();

            if(spread.Sheets.Any(x => x.Properties.Title == name))
                {
                    var id = spread.Sheets.First(x => x.Properties.Title == name).Properties.SheetId;
                    if(id != null) DeleteSheet(id.Value);
                }

            AddSheet(name);
        }

        private void AddSheet(string name)
        {
            var addSheetRequest = new AddSheetRequest();
            addSheetRequest.Properties = new SheetProperties();
            addSheetRequest.Properties.Title = name;
            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request {AddSheet = addSheetRequest});
            var req = _sheets.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _ownSpreadsheetId);
            req.Execute();
        }

        private void DeleteSheet(int id)
        {
            var deleSheetRequest = new DeleteSheetRequest();
            deleSheetRequest.SheetId = id;
            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request {DeleteSheet = deleSheetRequest});
            var req = _sheets.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _ownSpreadsheetId);
            req.Execute();
        }
    }
}
