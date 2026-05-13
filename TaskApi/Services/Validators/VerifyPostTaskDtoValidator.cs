using FluentValidation;
using TaskApi.Controllers;

namespace TaskApi.Services.Validators;

public class VerifyPostTaskDtoValidator : AbstractValidator<TasksController.PostTaskTdo>
{
    private const int MAX_TITLE_LENGTH = 200;
    private const int MIN_PRIORITY_LEVEL = 1;
    private const int MAX_PRIORITY_LEVEL = 3;
    
    public VerifyPostTaskDtoValidator()
    {
        RuleFor(verifyCode => verifyCode.Title)
            .NotEmpty()
            .WithMessage("Title should be not empty")
            .MaximumLength(MAX_TITLE_LENGTH)
            .WithMessage($"The description must not exceed {MAX_TITLE_LENGTH} characters.");

        RuleFor(verifyCode => verifyCode.Priority)
            .NotEmpty()
            .WithMessage("Priority should be not empty")
            .Must(priority => priority is >= MIN_PRIORITY_LEVEL and <= MAX_PRIORITY_LEVEL)
            .WithMessage($"The priority should be between {MIN_PRIORITY_LEVEL} and {MAX_PRIORITY_LEVEL}.");
    }
}