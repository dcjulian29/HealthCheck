﻿using System.Collections.Generic;
using System.Net.Mail;

namespace HealthCheck
{
    /// <summary>
    /// The result of an execution of the health check that may need to send out email messages.
    /// </summary>
    public interface IMailDeliveryStatus
    {
        /// <summary>
        /// Gets or sets the the collection of emails.
        /// </summary>
        IEnumerable<MailMessage> Emails { get; set; }
    }
}
