using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace APIBankingDiplom.GeneralUtilities.Models
{
    public class AvailabilityResult<T>
    {
        public ObjectResult Response { get; set; } = new ObjectResult(null);
        public T? Item { get; set; }

        [MemberNotNullWhen(true, nameof(Item))]
        public bool IsSuccess => Item is not null && Response.StatusCode is StatusCodes.Status200OK;
    }
}
