using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Entities;
using VDS.Helpers.Extensions;
using VDS.Security;
using VDS.Validation;

namespace VDS.AspNetCore.Mvc.Models
{
    public class DefaultErrorInfoConverter : IExceptionToErrorInfoConverter
    {
        public IExceptionToErrorInfoConverter Next { get; set; }

        public ErrorInfo Convert(Exception exception)
        {
            var errorInfo = CreateErrorInfoWithoutCode(exception);

            if (exception is IHasErrorCode)
            {
                errorInfo.Code = (exception as IHasErrorCode).Code;
            }

            return errorInfo;
        }

        private ErrorInfo CreateErrorInfoWithoutCode(Exception exception)
        {
//            if (SendAllExceptionsToClients)
//            {
//            }

            if (exception is AggregateException && exception.InnerException != null)
            {
                var aggException = exception as AggregateException;
                if (aggException.InnerException is UserFriendlyException ||
                    aggException.InnerException is ValidationException)
                {
                    exception = aggException.InnerException;
                }
            }

            if (exception is UserFriendlyException)
            {
                var userFriendlyException = exception as UserFriendlyException;
                return new ErrorInfo(userFriendlyException.Message, userFriendlyException.Details);
            }

            if (exception is ValidationException)
            {
                return new ErrorInfo("ValidationError")
                {
                    ValidationErrors = GetValidationErrorInfos(exception as ValidationException),
                    Details = GetValidationErrorNarrative(exception as ValidationException)
                };
            }

            if (exception is EntityNotFoundException)
            {
                var entityNotFoundException = exception as EntityNotFoundException;

                if (entityNotFoundException.EntityType != null)
                {
                    return new ErrorInfo(
                        string.Format(
                            "EntityNotFound",
                            entityNotFoundException.EntityType.Name,
                            entityNotFoundException.Id
                        )
                    );
                }

                return new ErrorInfo(
                    entityNotFoundException.Message
                );
            }

            if (exception is SecurityException)
            {
                var authorizationException = exception as SecurityException;
                return new ErrorInfo(authorizationException.Message);
            }

            return new ErrorInfo("InternalServerError");
        }

        private ErrorInfo CreateDetailedErrorInfoFromException(Exception exception)
        {
            var detailBuilder = new StringBuilder();

            AddExceptionToDetails(exception, detailBuilder);

            var errorInfo = new ErrorInfo(exception.Message, detailBuilder.ToString());

            if (exception is ValidationException)
            {
                errorInfo.ValidationErrors = GetValidationErrorInfos(exception as ValidationException);
            }

            return errorInfo;
        }

        private void AddExceptionToDetails(Exception exception, StringBuilder detailBuilder)
        {
            //Exception Message
            detailBuilder.AppendLine(exception.GetType().Name + ": " + exception.Message);

            //Additional info for UserFriendlyException
            if (exception is UserFriendlyException)
            {
                var userFriendlyException = exception as UserFriendlyException;
                if (!string.IsNullOrEmpty(userFriendlyException.Details))
                {
                    detailBuilder.AppendLine(userFriendlyException.Details);
                }
            }

            //Additional info for DomainValidationException
            if (exception is ValidationException)
            {
                var validationException = exception as ValidationException;
                if (validationException.ValidationErrors.Count > 0)
                {
                    detailBuilder.AppendLine(GetValidationErrorNarrative(validationException));
                }
            }

            //Exception StackTrace
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                detailBuilder.AppendLine("STACK TRACE: " + exception.StackTrace);
            }

            //Inner exception
            if (exception.InnerException != null)
            {
                AddExceptionToDetails(exception.InnerException, detailBuilder);
            }

            //Inner exceptions for AggregateException
            if (exception is AggregateException)
            {
                var aggException = exception as AggregateException;
                if (aggException.InnerExceptions.IsNullOrEmpty())
                {
                    return;
                }

                foreach (var innerException in aggException.InnerExceptions)
                {
                    AddExceptionToDetails(innerException, detailBuilder);
                }
            }
        }

        private ValidationErrorInfo[] GetValidationErrorInfos(ValidationException validationException)
        {
            var validationErrorInfos = new List<ValidationErrorInfo>();

            foreach (var validationResult in validationException.ValidationErrors)
            {
                foreach (var validationResultError in validationResult.Errors)
                {
                    var validationError = new ValidationErrorInfo(validationResultError.Message);

                    if (validationResultError.Name != null)
                    {
                        validationError.MemberName = validationResultError.Name.ToCamelCase();
                    }

                    validationErrorInfos.Add(validationError);
                }
            }

            return validationErrorInfos.ToArray();
        }

        private string GetValidationErrorNarrative(ValidationException validationException)
        {
            var detailBuilder = new StringBuilder();
            detailBuilder.AppendLine("ValidationNarrativeTitle");

            foreach (var validationResult in validationException.ValidationErrors)
            {
                foreach (var validationResultError in validationResult.Errors.ToList())
                {
                    detailBuilder.AppendFormat(" - {0}", validationResultError.Message);
                    detailBuilder.AppendLine();
                }
            }

            return detailBuilder.ToString();
        }
    }
}