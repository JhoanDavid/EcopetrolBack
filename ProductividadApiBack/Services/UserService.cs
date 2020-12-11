using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Productividad.Entities;
using Productividad.Helpers;
using Productividad.Models;
using ProductividadApiBack.Entities;
using ProductividadApiBack.Models;

namespace Productividad.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password, string strIp);
        ResponseDB CreateProponent(string strName, string strPhoneNumber, string strEmail, string strBusinessName, string strNIT, string strIp);
        string InsertAttachment(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID, string strFileUrl);
        string GetTokenAsync();
        string RequestFormDigest(string strAccessToken, string strSharePointUrl);
        ResponseDB UpdateProposal(int id, string strName, string strPhoneNumber, string strEmail, string strProponentType, string strBusinessName, string strNIT, string strWebSite, string strTechnologyName, string strTechnologyObject, string strFunctionality, string strValuePromise, string strApplicationSegment, string strSpecificApplicationArea, string strMaturityLevel, string strIp, string strProposalState, string strSIPROEID, string strSIPROEOption);
        ResponseDB updateDetailedForm(FormGMT17Model model, string strIp);
        FormGMT17Model GetDetailForm(string strId, string strIp);
        ResponseDB Desist(DesistModel model, string strIp);
        ResponseDB GetAllRows(string strTable);
        System.Threading.Tasks.Task<string> GetItemFileAsync(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID);
        System.Threading.Tasks.Task<string> DeleteItemFileAsync(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID, string FileTitle);
        string RememberPassword(string NIT, string ProposalIdCode, string strIp);
        ResponseDB Reactivate(DesistModel model, string strIp);
    }

    public class UserService : IUserService
    {

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string strUsername, string strPassword, string strIp)
        {

            Connection objConnection = new Connection();

            string strQueryCols = "[Id],[BusinessName],[DocumentNumber],[ProponentType],[ContactName],[PhoneNumber],[Email],[WebSite],[Password],[TechnologyName],[TechnologyObject],[Functionality],[ValuePromise],[ApplicationSegment],[SpecificApplicationArea],[ItemCode],[ProposalIdCode],[ProposalState],[Reactivated],[SubState],[MaturityLevel],[CreationDate],[SIPROEID],[SIPROEOption]";
            string strTable = "[tbl_Proponent]";
            string strCondition = "Email = @Email AND Password = @Password";

            string[] arrayParam = new string[2] { "@Email", "@Password" };
            string[] arrayValue = new string[2] { strUsername, strPassword };

            ResponseDB objResponseDB = objConnection.GetRespFromQuery(0, 1, strQueryCols, strTable, strCondition, arrayParam, arrayValue, "Id ASC", "DataTable", strIp);

            var user = new User();

            if (objResponseDB.Count > 0)
            {

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, objResponseDB.dtResult.Rows[0]["Id"].ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                user.ContactName = objResponseDB.dtResult.Rows[0]["ContactName"].ToString();
                user.Email = objResponseDB.dtResult.Rows[0]["Email"].ToString();
                user.PhoneNumber = objResponseDB.dtResult.Rows[0]["PhoneNumber"].ToString();
                user.BusinessName = objResponseDB.dtResult.Rows[0]["BusinessName"].ToString();
                user.NIT = objResponseDB.dtResult.Rows[0]["DocumentNumber"].ToString();
                user.ProponentType = objResponseDB.dtResult.Rows[0]["ProponentType"].ToString();
                user.WebSite = objResponseDB.dtResult.Rows[0]["WebSite"].ToString();
                user.TechnologyName = objResponseDB.dtResult.Rows[0]["TechnologyName"].ToString();
                user.TechnologyObject = objResponseDB.dtResult.Rows[0]["TechnologyObject"].ToString();
                user.Functionality = objResponseDB.dtResult.Rows[0]["Functionality"].ToString();
                user.ValuePromise = objResponseDB.dtResult.Rows[0]["ValuePromise"].ToString();
                user.ApplicationSegment = objResponseDB.dtResult.Rows[0]["ApplicationSegment"].ToString();
                user.SpecificApplicationArea = objResponseDB.dtResult.Rows[0]["SpecificApplicationArea"].ToString();
                user.MaturityLevel = objResponseDB.dtResult.Rows[0]["MaturityLevel"].ToString();
                user.ProposalState = objResponseDB.dtResult.Rows[0]["ProposalState"].ToString();
                user.Id = Int32.Parse(objResponseDB.dtResult.Rows[0]["Id"].ToString());
                user.ProposalIdCode = objResponseDB.dtResult.Rows[0]["ProposalIdCode"].ToString();
                user.Token = tokenHandler.WriteToken(token);
                user.CreationDate = objResponseDB.dtResult.Rows[0]["CreationDate"].ToString();
                user.ItemCode = objResponseDB.dtResult.Rows[0]["ItemCode"].ToString();
                user.Reactivated = objResponseDB.dtResult.Rows[0]["Reactivated"].ToString();
                user.SIPROEID = objResponseDB.dtResult.Rows[0]["SIPROEID"].ToString();
                user.SIPROEOption= objResponseDB.dtResult.Rows[0]["SIPROEOption"].ToString();
                return user;
            }
            else
            {
                return null;
            }
        }

        public ResponseDB CreateProponent(string strName, string strPhoneNumber, string strEmail, string strBusinessName, string strNIT, string strIp)
        {
            Connection objConnection = new Connection();
            string strQueryCols = "[ContactName],[PhoneNumber],[Email],[BusinessName],[DocumentNumber],[Password],[ProposalState]";
            string strTable = "[tbl_Proponent]";
            string strQuery = "";
            string strPassword = GeneratePassword();
            strQuery = "INSERT INTO " + strTable + " (" + strQueryCols + ") output INSERTED.ID VALUES(@ContactName,@PhoneNumber,@Email,@BusinessName,@DocumentNumber,@Password,@ProposalState)";
            string[] arrayParam = new string[7] { "@ContactName", "@PhoneNumber", "@Email", "@BusinessName", "@DocumentNumber", "@Password","@ProposalState" };
            string[] arrayValue = new string[7] { strName, strPhoneNumber, strEmail, strBusinessName, strNIT, strPassword, "En Registro" };

            ResponseDB objResponseDB = objConnection.InsData(strQuery, arrayParam, arrayValue, strIp);

            if (objResponseDB.Count > 0)
            {
                return objResponseDB;
            }
            else
            {
                return null;
            }
        }

        public ResponseDB UpdateProposal(int Id, string strName, string strPhoneNumber, string strEmail, string strProponentType, string strBusinessName, string strNIT, string strWebSite, string strTechnologyName, string strTechnologyObject, string strFunctionality, string strValuePromise, string strApplicationSegment, string strSpecificApplicationArea, string strMaturityLevel, string strIp, string strProposalState,string strSIPROEID, string strSIPROEOption)
        {
            Connection objConnection = new Connection();
            string strTable = "[tbl_Proponent]";
            string strQuery = "";
            DateTime localDate = DateTime.Now;
            string[] arrayParam=null;
            string[] arrayValue=null;
            String strType = "";
            if (strProposalState.Contains("En Recep"))
            {
                strType = "Proposal Submission";
            }
            else
            {
                strType = "Proposal Update";
            }
            if (strProponentType=="")
            {
                strQuery = "UPDATE " + strTable + "SET ContactName = @ContactName , PhoneNumber = @PhoneNumber , Email = @Email ," +
                        " BusinessName = @BusinessName , DocumentNumber = @DocumentNumber , WebSite = @WebSite , TechnologyName = @TechnologyName , " +
                        "TechnologyObject = @TechnologyObject, Functionality = @Functionality , ValuePromise = @ValuePromise ," +
                        "MaturityLevel = @MaturityLevel, ProposalState = @ProposalState, CreationDate=@CreationDate, SIPROEID=@SIPROEID, SIPROEOption=@SIPROEOption output INSERTED.ID WHERE Id = @Id";
               arrayParam = new string[16] { "@Id", "@ContactName", "@PhoneNumber", "@Email", "@BusinessName", "@DocumentNumber", "@WebSite", "@TechnologyName", "@TechnologyObject", "@Functionality", "@ValuePromise", "@MaturityLevel", "@ProposalState", "@CreationDate", "@SIPROEID", "SIPROEOption" };
               arrayValue = new string[16] { Id.ToString(), strName, strPhoneNumber, strEmail, strBusinessName, strNIT, strWebSite, strTechnologyName, strTechnologyObject, strFunctionality, strValuePromise, strMaturityLevel, strProposalState, localDate.ToString() ,strSIPROEID, strSIPROEOption };
            }else if (strApplicationSegment == "")
            {
                strQuery = "UPDATE " + strTable + "SET ContactName = @ContactName , PhoneNumber = @PhoneNumber , Email = @Email , ProponentType = @ProponentType ," +
                       " BusinessName = @BusinessName , DocumentNumber = @DocumentNumber , WebSite = @WebSite , TechnologyName = @TechnologyName , " +
                       "TechnologyObject = @TechnologyObject, Functionality = @Functionality , ValuePromise = @ValuePromise ," +
                       "MaturityLevel = @MaturityLevel, ProposalState = @ProposalState, CreationDate=@CreationDate, SIPROEID=@SIPROEID, SIPROEOption=@SIPROEOption output INSERTED.ID WHERE Id = @Id";
                arrayParam = new string[17] { "@Id", "@ContactName", "@PhoneNumber", "@Email", "@ProponentType", "@BusinessName", "@DocumentNumber", "@WebSite", "@TechnologyName", "@TechnologyObject", "@Functionality", "@ValuePromise", "@MaturityLevel", "@ProposalState", "@CreationDate", "@SIPROEID", "SIPROEOption" };
                arrayValue = new string[17] { Id.ToString(), strName, strPhoneNumber, strEmail, strProponentType, strBusinessName, strNIT, strWebSite, strTechnologyName, strTechnologyObject, strFunctionality, strValuePromise, strMaturityLevel, strProposalState, localDate.ToString(), strSIPROEID, strSIPROEOption };

            }
            else
            {
                strQuery = "UPDATE " + strTable + "SET ContactName = @ContactName , PhoneNumber = @PhoneNumber , Email = @Email , ProponentType = @ProponentType ," +
                       " BusinessName = @BusinessName , DocumentNumber = @DocumentNumber , WebSite = @WebSite , TechnologyName = @TechnologyName , " +
                       "TechnologyObject = @TechnologyObject, Functionality = @Functionality , ValuePromise = @ValuePromise , ApplicationSegment = @ApplicationSegment," +
                       "SpecificApplicationArea = @SpecificApplicationArea, MaturityLevel = @MaturityLevel, ProposalState = @ProposalState, CreationDate=@CreationDate, SIPROEID=@SIPROEID, SIPROEOption=@SIPROEOption output INSERTED.ID WHERE Id = @Id";
               arrayParam = new string[19] { "@Id", "@ContactName", "@PhoneNumber", "@Email", "@ProponentType", "@BusinessName", "@DocumentNumber", "@WebSite", "@TechnologyName", "@TechnologyObject", "@Functionality", "@ValuePromise", "@ApplicationSegment", "@SpecificApplicationArea", "@MaturityLevel", "@ProposalState", "@CreationDate", "@SIPROEID", "SIPROEOption" };
               arrayValue = new string[19] { Id.ToString(), strName, strPhoneNumber, strEmail, strProponentType, strBusinessName, strNIT, strWebSite, strTechnologyName, strTechnologyObject, strFunctionality, strValuePromise, strApplicationSegment, strSpecificApplicationArea, strMaturityLevel, strProposalState, localDate.ToString(), strSIPROEID, strSIPROEOption };

            }

            Console.Write("tipo de proponente "+strProponentType);
            ResponseDB objResponseDB = objConnection.UpdData(strQuery, arrayParam, arrayValue, strIp, strType);

            if (objResponseDB.Count > 0)
            {
                return objResponseDB;
            }
            else
            {
                return null;
            }
        }

        public string GeneratePassword()
        {
            Random rdn = new Random();
            string strChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890%$#@.-/()&$#!?=*";
            int length = strChars.Length;
            char cLetter;
            int PasswordLength = 12;
            string strRandomPassword = string.Empty;
            for (int i = 0; i < PasswordLength; i++)
            {
                cLetter = strChars[rdn.Next(length)];
                strRandomPassword += cLetter.ToString();
            }
            return strRandomPassword;
        }


        public ResponseDB updateDetailedForm(FormGMT17Model model, string strIp)
        {
            Connection objConnection = new Connection();
            string strTable = "[tbl_Proponent]";
            string strQuery = "";
            strQuery = "UPDATE " + strTable + "SET FPosition=@FPosition, FAddress=@FAddress, FTechnologyNameCaract=@FTechnologyNameCaract, FProsCompatibility=@FProsCompatibility, FSolComunProblem=@FSolComunProblem," +
                  "FISCompatibility=@FISCompatibility, FHomologation=@FHomologation, FAnswerNeed=@FAnswerNeed, FAdvantageDisadventage=@FAdvantageDisadventage, FCompanyRecommendUse=@FCompanyRecommendUse, FPotencialUsers=@FPotencialUsers, FOtherProcess=@FOtherProcess, " +
                  "FDifficulties=@FDifficulties, FAspects=@FAspects, FIntoProcess=@FIntoProcess, FForBusiness=@FForBusiness, FRisk=@FRisk, FAvailability=@FAvailability, FVisits=@FVisits, FStudyAvailability=@FStudyAvailability, FEvidence=@FEvidence," +
                  "FIndicators=@FIndicators, FAdequateResults=@FAdequateResults, FFactorsToModify=@FFactorsToModify, FImproveProductivity=@FImproveProductivity, FProductiveChange=@FProductiveChange, FAddStaff=@FAddStaff, FMaintenanceNeed=@FMaintenanceNeed," +
                  "FSufficientInventory=@FSufficientInventory, FInternalProcedure=@FInternalProcedure, FOperationChange=@FOperationChange, FImplications=@FImplications, FAffectsEnvironment=@FAffectsEnvironment," +
                  "FAStopConsecuences=@FAStopConsecuences, FIntellectualProperty=@FIntellectualProperty, FStatesProcedures=@FStatesProcedures, FProtectableAspects=@FProtectableAspects, FPropertyNegotiation=@FPropertyNegotiation, FPhoneFax=@FPhoneFax, ProposalState=@ProposalState output INSERTED.ID WHERE Id = @Id";

            string[] arrayParam = new string[41] { "@Id", "@FPosition",  "@FAddress",  "@FTechnologyNameCaract",  "@FProsCompatibility",  "@FSolComunProblem",
          "@FISCompatibility",  "@FHomologation",  "@FAnswerNeed",  "@FAdvantageDisadventage",  "@FCompanyRecommendUse", "@FPotencialUsers",  "@FOtherProcess",
          "@FDifficulties",  "@FAspects",  "@FIntoProcess","@FForBusiness", "@FRisk",  "@FAvailability",  "@FVisits",  "@FStudyAvailability",  "@FEvidence",
          "@FIndicators",  "@FAdequateResults",  "@FFactorsToModify",  "@FImproveProductivity",  "@FProductiveChange",  "@FAddStaff",  "@FMaintenanceNeed",
          "@FSufficientInventory",  "@FInternalProcedure", "@FOperationChange",  "@FImplications",  "@FAffectsEnvironment",
          "@FAStopConsecuences",  "@FIntellectualProperty",  "@FStatesProcedures", "@FProtectableAspects",  "@FPropertyNegotiation", "@FPhoneFax", "@ProposalState"};

            string[] arrayValue = new string[41] { model.Id.ToString(), model.FPosition,  model.FAddress,  model.FTechnologyNameCaract,  model.FProsCompatibility,  model.FSolComunProblem,
          model.FISCompatibility,  model.FHomologation,  model.FAnswerNeed,  model.FAdvantageDisadventage,  model.FCompanyRecommendUse,  model.FPotencialUsers,  model.FOtherProcess, model.FDifficulties,
          model.FAspects,  model.FIntoProcess, model.FForBusiness, model.FRisk,  model.FAvailability,  model.FVisits,  model.FStudyAvailability,  model.FEvidence,
          model.FIndicators,  model.FAdequateResults,  model.FFactorsToModify,  model.FImproveProductivity,  model.FProductiveChange,  model.FAddStaff,  model.FMaintenanceNeed,
          model.FSufficientInventory,  model.FInternalProcedure, model.FOperationChange,  model.FImplications,  model.FAffectsEnvironment, model.FAStopConsecuences,  model.FIntellectualProperty,  model.FStatesProcedures,
          model.FProtectableAspects,  model.FPropertyNegotiation, model.FPhoneFax, model.ProposalState };
            ResponseDB objResponseDB = new ResponseDB();
            if (model.ProposalState.Contains("Detallado"))
            {
                objResponseDB = objConnection.UpdData(strQuery, arrayParam, arrayValue, strIp, "Submit DetailedForm");
            }
            else
            {
                objResponseDB = objConnection.UpdData(strQuery, arrayParam, arrayValue, strIp, "Update DetailedForm");
            }
            if (objResponseDB.Count > 0)
            {
                return objResponseDB;
            }
            else
            {
                return null;
            }
        }



        public string InsertAttachment(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID, string strFileUrl)
        {
            try
            {
                var fileName = System.IO.Path.GetFileName(strFileUrl);
                string strRestUrl = strSharePointUrl + "/_api/web/lists/GetByTitle('" + strListName + "')/items(" + strElementID + ")/AttachmentFiles/add(FileName='" + fileName + "')";

                //Invoking REST API 
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strAccessToken);
                    httpClient.DefaultRequestHeaders.Add("X-RequestDigest", strRequestFormDigest);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var stream = System.IO.File.OpenRead(strFileUrl))
                    {
                        HttpResponseMessage httpResponse = httpClient.PostAsync(strRestUrl, new StreamContent(stream)).Result;

                        httpResponse.EnsureSuccessStatusCode();

                        string strJsonData = httpResponse.Content.ReadAsStringAsync().Result;
                        JObject jsonResult = JObject.Parse(strJsonData);

                        return jsonResult.ToString();
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al ingresar el elemento en la lista, error: " + e.Message);
            }
        }

        public string GetTokenAsync()
        {
            string strClientId = _appSettings.SharepointClientId; 
            string strClientSecret = _appSettings.SharePointClientSecret;
            string strTenantId = _appSettings.SharePointTenantId;
            string strTenantName = _appSettings.SharePointTenantName;

            string strRestUrl = "https://accounts.accesscontrol.windows.net/" + strTenantId + "/tokens/oauth/2";

            //Invoking REST API 
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();

                var values = new Dictionary<string, string>
                    {
                       { "Grant_type", "client_credentials" },
                       { "client_id", strClientId+"@"+strTenantId },
                       { "client_secret", strClientSecret },
                       { "resource", "00000003-0000-0ff1-ce00-000000000000/"+strTenantName+".sharepoint.com@"+strTenantId }
                    };

                var content = new FormUrlEncodedContent(values);

                HttpResponseMessage httpResponse = httpClient.PostAsync(strRestUrl, content).Result;
                httpResponse.EnsureSuccessStatusCode();

                string strJsonData = httpResponse.Content.ReadAsStringAsync().Result;
                JObject jsonResult = JObject.Parse(strJsonData);

                return jsonResult["access_token"].ToString();
            }
        }

        /// <summary>
        /// Método encargado de obtener el contexto de sharepoint para hacer peticiones.
        /// </summary>
        /// <param name="strAccessToken">AccesToken con permisos sobre el sitio para hacer la petición</param>
        /// <param name="strSharePointUrl">Url del sitio de sharepoint</param>
        /// <returns>string del contexto</returns>
        public string RequestFormDigest(string strAccessToken, string strSharePointUrl)
        {
            try
            {
                var objEndpointUrl = string.Format("{0}/_api/contextinfo", strSharePointUrl);
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strAccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var objResult = httpClient.PostAsync(objEndpointUrl, new StringContent(string.Empty)).Result;
                objResult.EnsureSuccessStatusCode();
                var objContent = objResult.Content.ReadAsStringAsync().Result;
                var objContentJson = JObject.Parse(objContent);
                return objContentJson["FormDigestValue"].ToString();
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener el RequestFormDigest, error: " + e.Message);
            }
        }

        public FormGMT17Model GetDetailForm(string strId, string strIp)
        {
            Connection objConnection = new Connection();

            string strQueryCols = "*";
            string strTable = "[tbl_Proponent]";
            string strCondition = "Id = @Id";

            string[] arrayParam = new string[1] { "@Id" };
            string[] arrayValue = new string[1] { strId };

            ResponseDB objResponseDB = objConnection.GetRespFromQuery(0, 1, strQueryCols, strTable, strCondition, arrayParam, arrayValue, "Id ASC", "DataTable", strIp);

            var objDetailForm = new FormGMT17Model();

            if (objResponseDB.Count > 0)
            {
                objDetailForm.FPosition = objResponseDB.dtResult.Rows[0]["FPosition"].ToString();
                objDetailForm.FAddress = objResponseDB.dtResult.Rows[0]["FAddress"].ToString();
                objDetailForm.FTechnologyNameCaract = objResponseDB.dtResult.Rows[0]["FTechnologyNameCaract"].ToString();
                objDetailForm.FProsCompatibility = objResponseDB.dtResult.Rows[0]["FProsCompatibility"].ToString();
                objDetailForm.FSolComunProblem = objResponseDB.dtResult.Rows[0]["FSolComunProblem"].ToString();
                objDetailForm.FISCompatibility = objResponseDB.dtResult.Rows[0]["FISCompatibility"].ToString();
                objDetailForm.FHomologation = objResponseDB.dtResult.Rows[0]["FHomologation"].ToString();
                objDetailForm.FAnswerNeed = objResponseDB.dtResult.Rows[0]["FAnswerNeed"].ToString();
                objDetailForm.FAdvantageDisadventage = objResponseDB.dtResult.Rows[0]["FAdvantageDisadventage"].ToString();
                objDetailForm.FCompanyRecommendUse = objResponseDB.dtResult.Rows[0]["FCompanyRecommendUse"].ToString();
                objDetailForm.FPotencialUsers = objResponseDB.dtResult.Rows[0]["FPotencialUsers"].ToString();
                objDetailForm.FOtherProcess = objResponseDB.dtResult.Rows[0]["FOtherProcess"].ToString();
                objDetailForm.FDifficulties = objResponseDB.dtResult.Rows[0]["FDifficulties"].ToString();
                objDetailForm.FAspects = objResponseDB.dtResult.Rows[0]["FAspects"].ToString();
                objDetailForm.FIntoProcess = objResponseDB.dtResult.Rows[0]["FIntoProcess"].ToString();
                objDetailForm.FForBusiness = objResponseDB.dtResult.Rows[0]["FForBusiness"].ToString();
                objDetailForm.FRisk = objResponseDB.dtResult.Rows[0]["FRisk"].ToString();
                objDetailForm.FAvailability = objResponseDB.dtResult.Rows[0]["FAvailability"].ToString();
                objDetailForm.FVisits = objResponseDB.dtResult.Rows[0]["FVisits"].ToString();
                objDetailForm.FStudyAvailability = objResponseDB.dtResult.Rows[0]["FStudyAvailability"].ToString();
                objDetailForm.FEvidence = objResponseDB.dtResult.Rows[0]["FEvidence"].ToString();
                objDetailForm.FIndicators = objResponseDB.dtResult.Rows[0]["FIndicators"].ToString();
                objDetailForm.FAdequateResults = objResponseDB.dtResult.Rows[0]["FAdequateResults"].ToString();
                objDetailForm.FFactorsToModify = objResponseDB.dtResult.Rows[0]["FFactorsToModify"].ToString();
                objDetailForm.FImproveProductivity = objResponseDB.dtResult.Rows[0]["FImproveProductivity"].ToString();
                objDetailForm.FProductiveChange = objResponseDB.dtResult.Rows[0]["FProductiveChange"].ToString();
                objDetailForm.FAddStaff = objResponseDB.dtResult.Rows[0]["FAddStaff"].ToString();
                objDetailForm.FMaintenanceNeed = objResponseDB.dtResult.Rows[0]["FMaintenanceNeed"].ToString();
                objDetailForm.FSufficientInventory = objResponseDB.dtResult.Rows[0]["FSufficientInventory"].ToString();
                objDetailForm.FInternalProcedure = objResponseDB.dtResult.Rows[0]["FInternalProcedure"].ToString();
                objDetailForm.FOperationChange = objResponseDB.dtResult.Rows[0]["FOperationChange"].ToString();
                objDetailForm.FImplications = objResponseDB.dtResult.Rows[0]["FImplications"].ToString();
                objDetailForm.FAffectsEnvironment = objResponseDB.dtResult.Rows[0]["FAffectsEnvironment"].ToString();
                objDetailForm.FAStopConsecuences = objResponseDB.dtResult.Rows[0]["FAStopConsecuences"].ToString();
                objDetailForm.FIntellectualProperty = objResponseDB.dtResult.Rows[0]["FIntellectualProperty"].ToString();
                objDetailForm.FStatesProcedures = objResponseDB.dtResult.Rows[0]["FStatesProcedures"].ToString();
                objDetailForm.FProtectableAspects = objResponseDB.dtResult.Rows[0]["FProtectableAspects"].ToString();
                objDetailForm.FPropertyNegotiation = objResponseDB.dtResult.Rows[0]["FPropertyNegotiation"].ToString();
                objDetailForm.FPhoneFax = objResponseDB.dtResult.Rows[0]["FPhoneFax"].ToString();
                return objDetailForm;
            }
            else
            {
                return null;
            }
        }

        public ResponseDB Desist(DesistModel model, string strIp)
        {
            Connection objConnection = new Connection();
            string strTable = "[tbl_Proponent]";
            string strQuery = "";
            strQuery = "UPDATE " + strTable + "SET ProposalState='Cerrado', RejectionObs=@RejectionObs output INSERTED.ID WHERE Id = @Id";

            string[] arrayParam = new string[2] { "@Id", "@RejectionObs" };

            string[] arrayValue = new string[2] { model.Id.ToString(), model.RejectionObs };

            ResponseDB objResponseDB = objConnection.UpdData(strQuery, arrayParam, arrayValue, strIp, "Desist");

            if (objResponseDB.Count > 0)
            {
                return objResponseDB;
            }
            else
            {
                return null;
            }
        }

        public ResponseDB GetAllRows(string strTable) {
            Connection objConnection = new Connection();
            string strQuery = "SELECT * FROM " + strTable;
            ResponseDB objResponseDB = objConnection.GetAll(strQuery, "DataTable");

            if (objResponseDB.dtResult == null)
            {
                return GetAllRows(strTable);
            }
            else
            {
                var length = objResponseDB.dtResult.Rows.Count;

                switch (strTable)
                {
                    case "tbl_ApplicationSegment":
                        ApplicationSegment[] arrayApplicationSegment = new ApplicationSegment[length];
                        for (int i = 0; i < length; i++)
                        {
                            ApplicationSegment objApplicationSegment = new ApplicationSegment();
                            objApplicationSegment.Cod = objResponseDB.dtResult.Rows[i]["Cod"].ToString();
                            objApplicationSegment.SegmentName = objResponseDB.dtResult.Rows[i]["SegmentName"].ToString();
                            arrayApplicationSegment[i] = objApplicationSegment;
                            objResponseDB.Data = arrayApplicationSegment;
                        }
                        break;

                    case "tbl_SpecificApplicationArea":
                        SpecificApplicationArea[] arraySpecificApplicationArea = new SpecificApplicationArea[length];
                        for (int i = 0; i < length; i++)
                        {
                            SpecificApplicationArea objSpecificApplicationArea = new SpecificApplicationArea();
                            objSpecificApplicationArea.Cod = objResponseDB.dtResult.Rows[i]["Cod"].ToString();
                            objSpecificApplicationArea.AreaName = objResponseDB.dtResult.Rows[i]["AreaName"].ToString();
                            objSpecificApplicationArea.ApplicationSegment = objResponseDB.dtResult.Rows[i]["ApplicationSegment"].ToString();
                            arraySpecificApplicationArea[i] = objSpecificApplicationArea;
                            objResponseDB.Data = arraySpecificApplicationArea;
                        }
                        break;
                }
                return objResponseDB;
            }
        }


        public async System.Threading.Tasks.Task<string> GetItemFileAsync(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID)
        {
            try
            {
                string strRestUrl = strSharePointUrl + "/_api/web/lists/GetByTitle('" + strListName + "')/items(" + strElementID + ")/AttachmentFiles";

                //Invoking REST API 
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strAccessToken);
                    httpClient.DefaultRequestHeaders.Add("X-RequestDigest", strRequestFormDigest);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    using HttpResponseMessage httpResponse = await httpClient.GetAsync(strRestUrl);
                    httpResponse.EnsureSuccessStatusCode();
                    string strJsonData = httpResponse.Content.ReadAsStringAsync().Result;
                    JObject jsonResult = JObject.Parse(strJsonData);
                    Console.WriteLine(jsonResult);
                    return jsonResult.ToString();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al ingresar el elemento en la lista, error: " + e.Message);
            }
        }

        public async System.Threading.Tasks.Task<string> DeleteItemFileAsync(string strAccessToken, string strRequestFormDigest, string strSharePointUrl, string strListName, string strElementID, string FileTitle)
        {
            try
            {
                string strRestUrl = strSharePointUrl + "/_api/web/lists/GetByTitle('" + strListName + "')/items(" + strElementID + ")/AttachmentFiles('" + FileTitle + "')";

                //Invoking REST API 
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strAccessToken);
                    httpClient.DefaultRequestHeaders.Add("X-RequestDigest", strRequestFormDigest);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                   
                    using HttpResponseMessage httpResponse = await httpClient.DeleteAsync(strRestUrl);
                    Console.WriteLine(httpResponse);
                    return "ok";
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al eliminar el elemento en la lista, error: " + e.Message);
            }
        }

        public string RememberPassword(string NIT, string Email, string strIp)
        {
            Connection objConnection = new Connection();

            string strQueryCols = "[Id],[BusinessName],[DocumentNumber],[ProponentType],[ContactName],[PhoneNumber],[Email],[WebSite],[Password],[TechnologyName],[TechnologyObject],[Functionality],[ValuePromise],[ApplicationSegment],[SpecificApplicationArea],[ItemCode],[ProposalIdCode],[ProposalState],[Reactivated],[SubState],[MaturityLevel],[CreationDate]";
            string strTable = "[tbl_Proponent]";
            string strCondition = "DocumentNumber = @DocumentNumber AND Email=@Email";

            string[] arrayParam = new string[2] { "@DocumentNumber", "@Email"};
            string[] arrayValue = new string[2] { NIT, Email };

            ResponseDB objResponseDB = objConnection.GetRespFromQuery(0, 1, strQueryCols, strTable, strCondition, arrayParam, arrayValue, "Id ASC", "DataTable", strIp);

            if (objResponseDB.Count > 0)
            {
                return "ok";
            }
            else
            {
                return null;
            }
        }

        public ResponseDB Reactivate(DesistModel model, string strIp)
        {
            Connection objConnection = new Connection();
            string strTable = "[tbl_Proponent]";
            string strQuery = "";
            strQuery = "UPDATE " + strTable + "SET ProposalState='En Registro', Reactivated='True', ReactivationObs=@ReactivationObs output INSERTED.ID WHERE Id = @Id";

            string[] arrayParam = new string[2] { "@Id", "@ReactivationObs" };

            string[] arrayValue = new string[2] { model.Id.ToString(), model.RejectionObs };

            ResponseDB objResponseDB = objConnection.UpdData(strQuery, arrayParam, arrayValue, strIp, "Reactivate");

            if (objResponseDB.Count > 0)
            {
                return objResponseDB;
            }
            else
            {
                return null;
            }
        }

    }
}