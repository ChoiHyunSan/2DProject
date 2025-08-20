using System.Runtime.InteropServices.JavaScript;
using APIServer.Models.Entity;
using SqlKata.Execution;

namespace APIServer.Repository.Implements;

partial class GameDb
{
    public async Task<UserAttendanceMonth> GetUserAttendance(long userId)
    {
        return await _queryFactory.Query(TABLE_USER_ATTENDANCE_MONTH)
            .Where(USER_ID, userId)
            .FirstOrDefaultAsync<UserAttendanceMonth>();
    }

    public async Task<bool> UpdateAttendanceToday(long userId, int day)
    {
        var result = await _queryFactory.Query(TABLE_USER_ATTENDANCE_MONTH)
            .Where(USER_ID, userId)
            .UpdateAsync(new
            {
                last_attendance_date = day,
                last_update_date = DateTime.UtcNow,
            });

        return result == 1;
    }
}