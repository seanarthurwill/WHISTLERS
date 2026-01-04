using Microsoft.EntityFrameworkCore;
using GroupsService.Data;
using GroupsService.Models;

namespace GroupsService.Services
{
    public interface IGroupService
    {
        Task<IEnumerable<Group>> GetAllGroupsAsync();
        Task<Group?> GetGroupByIdAsync(int id);
        Task<IEnumerable<Group>> GetGroupsBySportAsync(int sportId);
        Task<Group> CreateGroupAsync(Group group);
        Task<Group?> UpdateGroupAsync(int id, Group group);
        Task<bool> DeleteGroupAsync(int id);
        Task<bool> GroupExistsAsync(int id);
    }

    public interface IGroupMemberService
    {
        Task<IEnumerable<GroupMember>> GetGroupMembersAsync(int groupId);
        Task<IEnumerable<Group>> GetOfficialGroupsAsync(int officialId);
        Task<GroupMember?> GetGroupMemberByIdAsync(int id);
        Task<GroupMember> AddMemberToGroupAsync(GroupMember groupMember);
        Task<bool> RemoveMemberFromGroupAsync(int groupId, int officialId);
        Task<bool> IsOfficialInGroupAsync(int groupId, int officialId);
    }

    // ===== IMPLEMENTATIONS =====

    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                .OrderBy(g => g.GroupName)
                .ToListAsync();
        }

        public async Task<Group?> GetGroupByIdAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == id);
        }

        public async Task<IEnumerable<Group>> GetGroupsBySportAsync(int sportId)
        {
            return await _context.Groups
                .Where(g => g.SportId == sportId)
                .OrderBy(g => g.GroupName)
                .ToListAsync();
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            group.CreatedAt = DateTime.UtcNow;
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> UpdateGroupAsync(int id, Group group)
        {
            var existing = await _context.Groups.FindAsync(id);
            if (existing == null) return null;

            existing.GroupName = group.GroupName;
            existing.Description = group.Description;
            existing.SportId = group.SportId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return false;

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GroupExistsAsync(int id)
        {
            return await _context.Groups.AnyAsync(g => g.GroupId == id);
        }
    }

    public class GroupMemberService : IGroupMemberService
    {
        private readonly ApplicationDbContext _context;

        public GroupMemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroupMember>> GetGroupMembersAsync(int groupId)
        {
            return await _context.GroupMembers
                .Include(gm => gm.Group)
                .Where(gm => gm.GroupId == groupId)
                .OrderBy(gm => gm.JoinedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> GetOfficialGroupsAsync(int officialId)
        {
            return await _context.GroupMembers
                .Include(gm => gm.Group)
                .Where(gm => gm.OfficialId == officialId)
                .Select(gm => gm.Group)
                .ToListAsync();
        }

        public async Task<GroupMember?> GetGroupMemberByIdAsync(int id)
        {
            return await _context.GroupMembers
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync(gm => gm.GroupMemberId == id);
        }

        public async Task<GroupMember> AddMemberToGroupAsync(GroupMember groupMember)
        {
            groupMember.JoinedAt = DateTime.UtcNow;
            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();
            return groupMember;
        }

        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, int officialId)
        {
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.OfficialId == officialId);
            
            if (groupMember == null) return false;

            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsOfficialInGroupAsync(int groupId, int officialId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.OfficialId == officialId);
        }
    }
}
