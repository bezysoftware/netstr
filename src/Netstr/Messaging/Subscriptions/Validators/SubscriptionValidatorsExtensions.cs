﻿using Netstr.Messaging.Models;

namespace Netstr.Messaging.Subscriptions.Validators
{
    public static class SubscriptionValidatorsExtensions
    {
        /// <summary>
        /// Runs validations for the given subscription request and returns the first error or null.
        /// </summary>
        public static string? CanSubscribe(this IEnumerable<ISubscriptionRequestValidator> validators, string id, ClientContext context, IEnumerable<SubscriptionFilter> filters)
        {
            foreach (var validator in validators)
            {
                var error = validator.CanSubscribe(id, context, filters);
                if (error != null)
                {
                    return error;
                }
            }

            return null;
        }
    }
}