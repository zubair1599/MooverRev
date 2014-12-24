using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Web.UI;
using Business.Interfaces;
using Business.Models;
using Business.Repository;
using Business.Repository.Models;
using Business.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebGrease.Css.Extensions;
using WebServiceModels;
using Business.Repository;

namespace Moovers.WebServices.Controllers
{
    public class SmartMovesController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("v1/smart/images/")]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {
                LogRequest(Request, Request.RequestUri.ToString());
                LogRequest(Request, Request.Headers.GetValues("lookup").First());
                // Check whether the POST operation is MultiPart? 
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
        
                string lookup = Request.Headers.GetValues("lookup").First();

                var path = "~/SmartUploads/" + lookup;
                CreateSubPath(path);


                // Prepare CustomMultipartFormDataStreamProvider in which our multipart form // data will be loaded. 
                var fileSaveLocation = HttpContext.Current.Server.MapPath(path);

                var provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
                var files = new List<string>();

                // Read all contents of multipart message into CustomMultipartFormDataStreamProvider. 
                await Request.Content.ReadAsMultipartAsync(provider);


                foreach (MultipartFileData file in provider.FileData)
                {
                    files.Add(Path.GetFileName(file.LocalFileName));
                }
                // Send OK Response along with saved file names to the client. 
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        status = "success",
                        title = "",
                        message = fileSaveLocation

                    });

            }
            catch (System.Exception e)
            {
                return GetFaultMessage(e);
            }
        }
   
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/smart/files/")]
        public HttpResponseMessage GetFileNames(string lookup)
        {
            var path = "~/SmartUploads/" + lookup;
            var diFiles = new DirectoryInfo(HttpContext.Current.Server.MapPath(path));
            var files = new List<string>();
            
            diFiles.GetFiles().ForEach(f => files.Add(f.Name));

            var obj = new
            {
                filelist = files
            };
          
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    

                    data = files,

                });

        }
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/smart/file/")]
        public HttpResponseMessage GetImage(string lookup, string imgName)
        {
            var path = "~/SmartUploads/" + lookup;
            var diFiles = new DirectoryInfo(HttpContext.Current.Server.MapPath(path));
            var file = diFiles.EnumerateFiles().First(f=>f.Name.Equals(imgName));
            var data = ReadFile(file);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(data));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image" + file.Extension.Replace('.', '/'));
            return response;
        }
        private byte[] ReadFile(FileInfo file)
        {
            byte[] buffer = null;

            var fileStream = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);

            var binaryReader = new System.IO.BinaryReader(fileStream);

            var totalBytes = new System.IO.FileInfo(file.FullName).Length;

            buffer = binaryReader.ReadBytes((Int32)totalBytes);

            fileStream.Close();

            fileStream.Dispose();

            binaryReader.Close();
            return buffer;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/smart/quote/")]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    status = "Success",

                    message = "Hello Service",

                });
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("v1/smart/info/")]
        public HttpResponseMessage SaveInfo(HttpRequestMessage request)
        {
            try
            {
                string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

                LogRequest(request, data);

                //   return Request.CreateResponse(HttpStatusCode.OK, new { status = "Success",  incoming = data.SerializeToJson() });
                var data1 = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;
               
                
                var sb = new StringBuilder();
                sb.Append(data1);
                sb.Append(request.ToString());

                new EmployeeAuthenticationRepository(new CacheRepository()).LogRequest("patric", sb);
                var jObject = data1 as JObject;

                if (jObject == null)
                {
                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, new { status = "Failed", title = request, message = "json object is null", });
                }

                var smartmove = JsonConvert.DeserializeObject(jObject.ToString(), typeof(SmartMoveRep)) as SmartMoveRep;
                if (smartmove != null)
                {
                    var move = new SmartMove()
                    {
                        Name = smartmove.name,
                        Address1 = smartmove.from.GetBusinessAddress(),
                        Address2 = smartmove.to.GetBusinessAddress(),
                        Email = smartmove.email,
                        From_latlng = smartmove.from_latlng!=null?smartmove.from_latlng.latitude+","+smartmove.from_latlng.longitude:"",
                        To_latlng = smartmove.to_latlng != null ? smartmove.to_latlng.latitude + "," + smartmove.to_latlng.longitude : "",
                        Phone = smartmove.telephone,
                        EstimatedMoveDate = smartmove.move_date,
                        AddedDate = DateTime.Now,
                        SmartMoveJSON = data1.SerializeToJson()
                    };
                    var repo = new SmartMoveRepository();
                    repo.Add(move);
                    repo.Save();
                var   savedMove =  repo.Get(move.SmartMoveID);
                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "Success", lookup= savedMove.SmartMoveLookUp});
                }
                return Request.CreateResponse(HttpStatusCode.Conflict, new { status = "Failed", message= "Failed to deserialized" });
            }
            catch (Exception ex)
            {
              return  GetFaultMessage(ex);
            }
        }

        private static void LogRequest(HttpRequestMessage request, string data)
        {
            var sb1 = new StringBuilder();
            sb1.Append(data);
            sb1.Append(request.ToString());

            new EmployeeAuthenticationRepository(new CacheRepository()).LogRequest("patric", sb1);
        }

        private HttpResponseMessage GetFaultMessage(Exception ex)
        {
            int line = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    status = "Failed",
                    title = ex.Message + ":inner exception: " + ex.InnerException,
                    message = ex.StackTrace,
                    lineno = line,
                    data = new { updated = false }
                });
        }

        private static void CreateSubPath(string path)
        {
            bool isExists = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path));

            if (!isExists)
                System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

        }
    }

    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProvider(string path) : base(path)
        {
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return headers.ContentDisposition.FileName;
          
        }

    }
}