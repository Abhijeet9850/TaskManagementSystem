import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Password: min 8 chars, at least one uppercase, one lowercase, one number.
export const strongPasswordValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string;
  if (!value) return null;

  const pattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
  return pattern.test(value) ? null : { weakPassword: true };
};

// Cross-field: confirmPassword must match password.
export function passwordsMatchValidator(passwordKey: string, confirmKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordKey)?.value;
    const confirm = group.get(confirmKey)?.value;

    if (!password || !confirm) return null;
    return password === confirm ? null : { passwordMismatch: true };
  };
}

// Cross-field: dueDate must not be earlier than startDate.
export function dueDateNotBeforeStartValidator(startKey: string, dueKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value;
    const due = group.get(dueKey)?.value;

    if (!start || !due) return null;
    return new Date(due) >= new Date(start) ? null : { dueBeforeStart: true };
  };
}
