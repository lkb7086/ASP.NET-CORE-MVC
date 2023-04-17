using tubaNWeb.CommonClass;
using tubaNWeb.Models.Unreal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace tubaNWeb.Controllers
{
    //[Route("[controller]/[action]")]
    [Route("api/[controller]")]
    [ApiController]
    public class UnrealController : ControllerBase
    {
        [HttpGet]
        public void Get()
        {
        }

        [HttpPost]
        public string Post([FromBody] object data)
        {
            JObject recvJObject = JObject.Parse(data.ToString());
            string type;
            try
            {
                type = recvJObject["Type"].ToString();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Log(ex.ToString());
                return "Type:Error / Result:" + ex.ToString();
            }

            UnrealModel unrealModel = new UnrealModel();
            JObject sendJObject = null;
            switch (type)
            {
                case "ImportEpisode":
                    sendJObject = unrealModel.ImportEpisode(recvJObject);
                    break;

                case "ImportSceneCut":
                    sendJObject = unrealModel.ImportSceneCut(recvJObject);
                    break;

                case "ChangeState":
                    sendJObject = unrealModel.ChangeState(recvJObject);
                    break;

                case "UnrealLog":
                    sendJObject = unrealModel.UnrealLog(recvJObject);
                    break;

                case "TableName":
                    sendJObject = unrealModel.TableName();
                    break;

                default:
                    LogManager.Instance.Log(string.Format("Unreal Type Error: type:{0}", type));
                    break;
            }

            if (sendJObject != null && sendJObject.Count > 0)
                return sendJObject.ToString();
            else
                return "Error";
        }


    }
}
