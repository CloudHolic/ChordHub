using ChordHub.Api.Shared.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChordHub.Api.Shared.Hubs;

[Authorize]
public class AnalysisProgressHub : Hub
{
    public async Task JoinAnalysisGroup(string jobId)
    {
        var userId = Context.User?.GetUserId().ToString();
        if (string.IsNullOrEmpty(userId))
            return;

        // TODO: 이 User가 해당 Job의 실제 소유자인지 확인해야 함.
        var groupName = $"analysis_{jobId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveAnalysisGroup(string jobId)
    {
        var groupName = $"analysis_{jobId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
