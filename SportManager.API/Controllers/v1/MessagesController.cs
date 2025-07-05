//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SportManager.Application.ChatHubs.Commands;

//namespace SportManager.API.Controllers.v1;

//[Authorize]
//[ApiController]
////[Route("api/messages")]
////public class MessagesController : ControllerBase
////{
////    private readonly IMediator _mediator;

////    public MessagesController(IMediator mediator)
////    {
////        _mediator = mediator;
////    }

////    [HttpPost]
////    public async Task<IActionResult> SendMessage([FromBody] CreateMessageCommand command)
////    {
////        var result = await _mediator.Send(command);
////        return Ok(result);
////    }
////    [HttpGet]
////    public async Task<IActionResult> GetConversations()
////    {
////        var result = await _mediator.Send(new GetConversationsQuery());
////        return Ok(result);
////    }

////    [HttpGet("{conversationId}")]
////    public async Task<IActionResult> GetConversationDetail(Guid conversationId)
////    {
////        var result = await _mediator.Send(new GetConversationDetailQuery
////        {
////            ConversationId = conversationId
////        });
////        return Ok(result);
////    }
////}