using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyWeb.Model;
using SurveyWeb.Services;

namespace SurveyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {

        private readonly ISurveyServices _surveyServices;

        public SurveyController(ISurveyServices surveyServices)
        {
            _surveyServices = surveyServices;
        }

        [HttpGet]
        [Route("version")]
        public ActionResult GetVersion()
        {
            return Ok("Version 0.001");
        }

        [HttpGet]
        [Route("question")]
        public async Task<ActionResult<List<SurveyQuestion>>> GetQuestion()
        {
            var surveys = await _surveyServices.GetSurveysAsync();
            return Ok(surveys);
        }
        [HttpGet]
        [Route("question/{templateid}")]
        public async Task<ActionResult<List<SurveyQuestion>>> GetQuestionByTemplateId(int templateid)
        {
            var surveys = await _surveyServices.GetQuestionByTemplateId(templateid);
            return Ok(surveys);
        }

        [HttpPost]
        [Route("question")]
        public async Task<ActionResult<int>> PostQuestion([FromBody] SurveyQuestion question)
        {
            var surveys = await _surveyServices.CreateQuestion(question);
            return Ok(surveys);
        }

        [HttpPost]
        [Route("answer/{templateid}")]
        public async Task<ActionResult> PostAnswer([FromBody] List<SurveyAnswer> answers, int templateid)
        {
            var surveys = await _surveyServices.SubmitSurvey(answers, templateid);
            return Ok(surveys);
        }

        [HttpGet]
        [Route("submitted")]
        public async Task<ActionResult<List<SurveyQuestion>>> GetSurveys()
        {
            var surveys = await _surveyServices.GetSurveyAnswer();
            return Ok(surveys);
        }

        [HttpGet]
        [Route("submitted/{id}")]
        public async Task<ActionResult<SurveyBody>> GetSurveysById(int id)
        {
            var surveys = await _surveyServices.GetSurveysById(id);
            return Ok(surveys);
        }

        [HttpGet]
        [Route("template")]
        public async Task<ActionResult> GetTemplate()
        {
            var surveys = await _surveyServices.GetTemplate();
            return Ok(surveys);
        }

        [HttpGet]
        [Route("questionbytemplate/{id}")]
        public async Task<ActionResult> GetQuestionByTemplate(int id)
        {
            var surveys = await _surveyServices.GetQuestionByTemplate(id);
            return Ok(surveys);
        }

        [HttpPost]
        [Route("template")]
        public async Task<ActionResult<int>> CreateTemplate([FromBody] SurveyTemplate template)
        {
            var surveys = await _surveyServices.CreateTemplate(template);
            return Ok(surveys);
        }
    }
}
