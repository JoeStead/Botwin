﻿namespace Botwin.Samples
{
    using System.Collections.Generic;
    using System.Linq;
    using samples.Features.FunctionalProgramming;

    public static class ListDirectorsRoute
    {
        public delegate IEnumerable<Director> ListDirectorsHandler();
        public delegate IEnumerable<Director> ListDirectors();
        public delegate bool UserAllowed();

        public static IEnumerable<Director> Handle(ListDirectors listDirectors, UserAllowed userAllowed, SharedDelegateExample sharedDelegateExample)
        {
            var truthy = sharedDelegateExample();

            var currentUserAllowed = userAllowed();
            if (!currentUserAllowed)
            {
                return null;
            }

            var directors = listDirectors();

            return directors ?? Enumerable.Empty<Director>();
        }
    }
}