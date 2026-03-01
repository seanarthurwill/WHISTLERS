using Microsoft.AspNetCore.Mvc;
using Whistl3rApi.Models;
using Whistl3rApi.Services;

namespace Whistl3rApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetAll()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetById(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpGet("sport/{sportId}")]
        public async Task<ActionResult<IEnumerable<Group>>> GetBySport(int sportId)
        {
            var groups = await _groupService.GetGroupsBySportAsync(sportId);
            return Ok(groups);
        }

        [HttpPost]
        public async Task<ActionResult<Group>> Create([FromBody] Group group)
        {
            var created = await _groupService.CreateGroupAsync(group);
            return CreatedAtAction(nameof(GetById), new { id = created.GroupId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Group>> Update(int id, [FromBody] Group group)
        {
            var updated = await _groupService.UpdateGroupAsync(id, group);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _groupService.DeleteGroupAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/groups/{groupId}/members")]
    public class GroupMembersController : ControllerBase
    {
        private readonly IGroupMemberService _groupMemberService;

        public GroupMembersController(IGroupMemberService groupMemberService)
        {
            _groupMemberService = groupMemberService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupMember>>> GetGroupMembers(int groupId)
        {
            var members = await _groupMemberService.GetGroupMembersAsync(groupId);
            return Ok(members);
        }

        [HttpPost]
        public async Task<ActionResult<GroupMember>> AddMember(int groupId, [FromBody] GroupMember groupMember)
        {
            groupMember.GroupId = groupId;
            var created = await _groupMemberService.AddMemberToGroupAsync(groupMember);
            return CreatedAtAction(nameof(GetGroupMembers), new { groupId }, created);
        }

        [HttpDelete("{officialId}")]
        public async Task<IActionResult> RemoveMember(int groupId, int officialId)
        {
            var removed = await _groupMemberService.RemoveMemberFromGroupAsync(groupId, officialId);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}
