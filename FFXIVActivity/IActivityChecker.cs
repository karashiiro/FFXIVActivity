using System;
using System.Threading.Tasks;

namespace FFXIVActivity
{
    public interface IActivityChecker
    {
        /// <summary>
        ///     Gets the last activity time of a user using the provided Lodestone ID.
        /// </summary>
        /// <param name="lodestoneId">The user's Lodestone ID.</param>
        Task<DateTime> GetLastActivityTime(ulong lodestoneId);

        /// <summary>
        ///     Gets the last activity time of the user with the provided character name and world.
        /// </summary>
        /// <param name="name">The character's name.</param>
        /// <param name="world">The character's world.</param>
        Task<DateTime> GetLastActivityTime(string name, string world);
    }
}
