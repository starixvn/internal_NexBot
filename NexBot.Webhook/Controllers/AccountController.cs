namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class AccountController(IAccountService accountService) : Controller
    {
        //[Authorize(Roles = "Bot")]
        [Route("Index")]
        public async Task<JsonResult> Get()
        {
            var accounts = await accountService.GetAllConversationUser().ConfigureAwait(false);

            return Json(accounts);
        }

        [Route("GetRegisteredConversationByMessageCategory")]
        public async Task<JsonResult> GetRegisteredConversationByMessageCategory(int messageCategoryId)
        {
            var accounts = await accountService.GetRegisteredConversationByMessageCategory(messageCategoryId).ConfigureAwait(false);

            return Json(accounts);
        }

        //[Authorize(Roles = "Bot")]
        [Route("GetIgnoredAccountByMessageCategory")]
        public async Task<JsonResult> GetIgnoredAccountByMessageCategory(int messageCategoryId)
        {
            var accounts = await accountService.GetIgnoredAccountByMessageCategory(messageCategoryId).ConfigureAwait(false);

            return Json(accounts);
        }

        [Route("GetAll")]
        public async Task<JsonResult> GetAll()
        {
            var accounts = await accountService.GetAllConversationUser().ConfigureAwait(false);

            return Json(accounts);
        }
    }
}