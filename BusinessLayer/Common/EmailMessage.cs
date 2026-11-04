namespace BusinessLayer.Common;

public record EmailMessage(string ToAddress, string Subject, string HtmlBody);
