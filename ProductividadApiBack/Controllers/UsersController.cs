using Productividad.Services;
using Productividad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductividadApiBack.Models;
using Productividad.Entities;
using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Options;
using Productividad.Helpers;

namespace Productividad.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        public static IWebHostEnvironment _env;
        private IUserService _userService;
        private readonly AppSettings _appSettings;
        public UsersController(IUserService userService, IWebHostEnvironment env, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _env = env;
            _appSettings = appSettings.Value;
        }

        //ID BackLog 24553 Opción de acceso y/o registro (Portal web ECP)
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objUser = _userService.Authenticate(model.Username, model.Password, strIp);

            Response objResponse = new Response();

            if (objUser == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "Usuario o contraseña incorrectos";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Token = objUser.Token;
                objResponse.Data = objUser;
                return Ok(objResponse);
            }
        }

        //ID BackLog 24553 Opción de acceso y/o registro (Portal web ECP)
        [AllowAnonymous]
        [HttpPost("createProponent")]
        public IActionResult InsertProponent([FromBody]ProponentModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objUser = _userService.CreateProponent(model.ContactName, model.PhoneNumber, model.Email, model.BusinessName, model.NIT, strIp);

            Response objResponse = new Response();

            if (objUser == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "No se pudo crear el Proponente";
                return BadRequest(objResponse);
            }
            else
            {

                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";

                return Ok(objResponse);
            }
        }

        //ID BackLog 24247 Presentar propuesta tecnológica
        [AllowAnonymous]
        [HttpPost("uploadFile")]
        public async Task<IActionResult> UploadFile([FromForm]FileModel objFile)
        {
            Response objResponse = new Response();
            try
            {
                for (int i = 0; i < objFile.files.Length; i++)
                {
                    if (objFile.files[i].Length > 0)
                    {
                        if (!Directory.Exists(_env.WebRootPath + "\\upload\\"))
                        {
                            Directory.CreateDirectory(_env.WebRootPath + "\\upload\\");
                        }
                        using (FileStream fileStream = System.IO.File.Create(_env.WebRootPath + "\\upload\\" + objFile.files[i].FileName))
                        {
                            objFile.files[i].CopyTo(fileStream);
                            fileStream.Flush();
                            fileStream.Close();
                            string strPath = _env.WebRootPath + "\\upload\\" + objFile.files[i].FileName;
                            string strSharePointUrl = _appSettings.SharePointUrl;
                            string strListName = _appSettings.SPListName;
                            var strAccessToken = _userService.GetTokenAsync();
                            string strRequestFormDigest = _userService.RequestFormDigest(strAccessToken, strSharePointUrl);
                            var obj = _userService.InsertAttachment(strAccessToken, strRequestFormDigest, strSharePointUrl, strListName, objFile.ElementId, strPath);
                            System.IO.File.Delete(strPath);
                        }
                    }
                }
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                return Ok(objResponse);
            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "error, " + ex.Message.ToString(); ;
                return BadRequest(objResponse);
            }
        }

        //ID BackLog 24247 Presentar propuesta tecnológica
        [Authorize]
        [HttpPost("updateProposal")]
        public IActionResult updateProposal([FromBody]ProponentModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objResponseDB = _userService.UpdateProposal(model.Id, model.ContactName, model.PhoneNumber, model.Email, model.ProponentType, model.BusinessName, model.NIT, model.WebSite, model.TechnologyName, model.TechnologyObject, model.Functionality, model.ValuePromise, model.ApplicationSegment, model.SpecificApplicationArea, model.MaturityLevel, strIp, model.ProposalState,model.SIPROEID,model.SIPROEOption);
            Response objResponse = new Response();
            if (objResponseDB == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "No se pudo actualizar";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objResponseDB.Data;
                return Ok(objResponse);
            }
        }

        //ID BackLog 24317 Solicitud de más información al proponente
        [Authorize]
        [HttpPost("updateFormGMT17")]
        public IActionResult updateFormGMT17([FromBody]FormGMT17Model model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objResponseDB = _userService.updateDetailedForm(model, strIp);
            Response objResponse = new Response();
            if (objResponseDB == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "No se pudo actualizar";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objResponseDB.Data;
                return Ok(objResponse);
            }
        }

        //ID BackLog 24317 Solicitud de más información al proponente
        [AllowAnonymous]
        [HttpPost("getDetailForm")]
        public IActionResult getDetailForm([FromBody]ProponentModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objDetailForm = _userService.GetDetailForm(model.Id.ToString(), strIp);
            Response objResponse = new Response();
            if (objDetailForm == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "error al traer los datos";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objDetailForm;
                return Ok(objResponse);
            }
        }

        //ID BackLog 24317 Solicitud de más información al proponente
        [AllowAnonymous]
        [HttpPost("desist")]
        public IActionResult Desist([FromBody]DesistModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objDesist = _userService.Desist(model, strIp);
            Response objResponse = new Response();
            if (objDesist == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "error al realizar la operación";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objDesist;
                return Ok(objResponse);
            }
        }

        [Authorize]
        [HttpPost("sesionValidate")]
        public IActionResult sesionValidate()
        {
            Response objResponse = new Response();
            objResponse.Resp = true;
            objResponse.Type = "Success";
            objResponse.Msg = "Ok";
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("GetAll")]
        public IActionResult GetAll([FromBody] TableModel objTable)
        {
            var objData = _userService.GetAllRows(objTable.Table);

            Response objResponse = new Response();

            if (objData == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "No se encontraron datos";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objData.Data;
                return Ok(objResponse);
            }


        }


        [AllowAnonymous]
        [HttpPost("GetFile")]
        public async Task<IActionResult> GetFileAsync([FromBody] FileModel ObjFile)
        {
            string strSharePointUrl = _appSettings.SharePointUrl;
            string strListName = _appSettings.SPListName; 
              var strAccessToken = _userService.GetTokenAsync();
            string strRequestFormDigest = _userService.RequestFormDigest(strAccessToken, strSharePointUrl);

            string jsonResponse= await _userService.GetItemFileAsync(strAccessToken, strRequestFormDigest, strSharePointUrl, strListName, ObjFile.ElementId);
            return Ok(jsonResponse);
        }

        [AllowAnonymous]
        [HttpPost("DeleteFile")]
        public async Task<IActionResult> DeleteFileAsync([FromBody] FileModel ObjFile)
        {
            string strSharePointUrl = _appSettings.SharePointUrl;
            string strListName = _appSettings.SPListName;
            var strAccessToken = _userService.GetTokenAsync();
            string strRequestFormDigest = _userService.RequestFormDigest(strAccessToken, strSharePointUrl);

            string jsonResponse = await _userService.DeleteItemFileAsync(strAccessToken, strRequestFormDigest, strSharePointUrl, strListName, ObjFile.ElementId, ObjFile.FileTitle);
            return Ok(jsonResponse);
        }

        [AllowAnonymous]
        [HttpPost("RememberPassword")]
        public IActionResult RememberPassword([FromBody]ProponentModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objUser = _userService.RememberPassword(model.NIT, model.Email, strIp);

            Response objResponse = new Response();

            if (objUser == "ok")
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objUser;
                return Ok(objResponse);
            }
            else
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "No se encontró el usuario";
                return BadRequest(objResponse);
            }
           
        }

        [AllowAnonymous]
        [HttpPost("Reactivate")]
        public IActionResult Reactivate([FromBody]DesistModel model)
        {
            string strIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
            var objDesist = _userService.Reactivate(model, strIp);
            Response objResponse = new Response();
            if (objDesist == null)
            {
                objResponse.Resp = false;
                objResponse.Type = "Warning";
                objResponse.Msg = "error al realizar la operación";
                return BadRequest(objResponse);
            }
            else
            {
                objResponse.Resp = true;
                objResponse.Type = "Success";
                objResponse.Msg = "Ok";
                objResponse.Data = objDesist;
                return Ok(objResponse);
            }
        }


    }
}