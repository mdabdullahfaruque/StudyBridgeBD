import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { CheckboxModule } from 'primeng/checkbox';
import { RadioButtonModule } from 'primeng/radiobutton';
import { DatePickerModule } from 'primeng/datepicker';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

export interface FormFieldOption {
  label: string;
  value: any;
  disabled?: boolean;
}

export interface FormFieldValidation {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  email?: boolean;
  pattern?: string;
  custom?: (control: AbstractControl) => { [key: string]: any } | null;
}

export interface FormField {
  key: string;
  type: 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'multiselect' | 'checkbox' | 'radio' | 'date' | 'toggle' | 'hidden';
  label?: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  readonly?: boolean;
  options?: Array<{ label: string; value: any }>;
  validation?: FormFieldValidation;
  conditionalLogic?: FormFieldConditionalLogic;
  cssClass?: string;
  width?: string;
  gridColumn?: string;
  helpText?: string;
  rows?: number;
  min?: number;
  max?: number;
  step?: number;
  checkboxLabel?: string;
  toggleLabel?: string;
  defaultValue?: any;
  hidden?: boolean;
  dependsOn?: string;
  dependsOnValue?: any;
  colSpan?: number;
}

export interface FormFieldConditionalLogic {
  dependsOn: string;
  dependsOnValue: any;
  action: 'show' | 'hide' | 'enable' | 'disable';
}

export interface FormConfig {
  title?: string;
  description?: string;
  fields?: FormField[];
  layout?: 'vertical' | 'horizontal' | 'grid' | 'inline';
  columns?: number;
  showSubmitButton?: boolean;
  showResetButton?: boolean;
  submitButtonText?: string;
  resetButtonText?: string;
  submittingText?: string;
  validationMode?: 'onChange' | 'onSubmit' | 'onBlur';
  cssClass?: string;
  actionButtonAlignment?: 'left' | 'center' | 'right' | 'space-between' | 'full-width';
  readonly?: boolean;
  showLabels?: boolean;
  showValidationSummary?: boolean;
}

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    SelectModule,
    MultiSelectModule,
    CheckboxModule,
    RadioButtonModule,
    DatePickerModule,
    ToggleSwitchModule,
    ButtonModule,
    MessageModule
  ],
  templateUrl: './dynamic-form.component.html',
  styleUrl: './dynamic-form.component.scss'
})
export class DynamicFormComponent implements OnInit, OnChanges {
  @Input() fields: FormField[] = [];
    @Input() config: FormConfig = {
    layout: 'vertical',
    columns: 1,
    showSubmitButton: true,
    showResetButton: false,
    submitButtonText: 'Submit',
    resetButtonText: 'Reset',
    submittingText: 'Submitting...',
    validationMode: 'onChange',
    actionButtonAlignment: 'right',
    showLabels: true,
    cssClass: '',
    showValidationSummary: false,
    readonly: false
  };

  @Input() initialData: any = {};
  @Input() disabled = false;

  @Output() formSubmit = new EventEmitter<any>();
  @Output() fieldChange = new EventEmitter<{ field: string; value: any; formValue: any }>();
  @Output() formValidationChange = new EventEmitter<boolean>();

  form!: FormGroup;
  validationMessages: any = {};
  formMessages: Array<{ severity: 'success' | 'info' | 'warning' | 'error', text: string }> = [];
  isSubmitting = false;

  get dynamicForm(): FormGroup {
    return this.form;
  }

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fields'] && !changes['fields'].isFirstChange()) {
      this.buildForm();
    }
    if (changes['initialData'] && !changes['initialData'].isFirstChange()) {
      this.loadInitialData();
    }
  }

  private buildForm(): void {
    const formControls: any = {};
    
    this.fields.forEach(field => {
      const validators = this.buildValidators(field.validation);
      const initialValue = this.getInitialValue(field);
      
      formControls[field.key] = [
        { value: initialValue, disabled: field.disabled || this.config.readonly },
        validators
      ];
    });

    this.form = this.fb.group(formControls);
    
    // Set up field change listeners
    this.setupFieldListeners();
    
    // Set up validation listeners
    this.form.statusChanges.subscribe(status => {
      this.formValidationChange.emit(status === 'VALID');
    });
  }

  private buildValidators(validation?: FormFieldValidation) {
    const validators = [];
    
    if (validation) {
      if (validation.required) validators.push(Validators.required);
      if (validation.minLength) validators.push(Validators.minLength(validation.minLength));
      if (validation.maxLength) validators.push(Validators.maxLength(validation.maxLength));
      if (validation.min !== undefined) validators.push(Validators.min(validation.min));
      if (validation.max !== undefined) validators.push(Validators.max(validation.max));
      if (validation.email) validators.push(Validators.email);
      if (validation.pattern) validators.push(Validators.pattern(validation.pattern));
      if (validation.custom) validators.push(validation.custom);
    }
    
    return validators;
  }

  private getInitialValue(field: FormField): any {
    if (this.initialData && this.initialData[field.key] !== undefined) {
      return this.initialData[field.key];
    }
    return field.defaultValue || this.getDefaultValueByType(field.type);
  }

  private getDefaultValueByType(type: string): any {
    switch (type) {
      case 'checkbox':
      case 'switch':
        return false;
      case 'multiselect':
        return [];
      case 'number':
        return 0;
      case 'date':
        return null;
      default:
        return '';
    }
  }

  private setupFieldListeners(): void {
    Object.keys(this.form.controls).forEach(fieldKey => {
      this.form.get(fieldKey)?.valueChanges.subscribe(value => {
        this.fieldChange.emit({
          field: fieldKey,
          value: value,
          formValue: this.form.value
        });
        
        // Handle conditional fields
        this.updateConditionalFields();
      });
    });
  }

  private updateConditionalFields(): void {
    this.fields.forEach(field => {
      if (field.dependsOn) {
        const dependsOnControl = this.form.get(field.dependsOn);
        const currentControl = this.form.get(field.key);
        
        if (dependsOnControl && currentControl) {
          const shouldShow = dependsOnControl.value === field.dependsOnValue;
          
          if (shouldShow) {
            currentControl.enable();
            field.hidden = false;
          } else {
            currentControl.disable();
            field.hidden = true;
            currentControl.setValue(this.getDefaultValueByType(field.type));
          }
        }
      }
    });
  }

  get visibleFields(): FormField[] {
    return this.actualFields.filter(field => !field.hidden);
  }

  get actualFields(): FormField[] {
    return this.config.fields || this.fields;
  }

  private loadInitialData(): void {
    if (this.initialData && this.form) {
      this.form.patchValue(this.initialData);
    }
  }

  // Public methods
  submit(): void {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value);
    } else {
      this.markAllFieldsAsTouched();
    }
  }

  reset(): void {
    this.form.reset();
    this.fields.forEach(field => {
      const control = this.form.get(field.key);
      if (control) {
        control.setValue(this.getInitialValue(field));
      }
    });
  }

  markAllFieldsAsTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      this.form.get(key)?.markAsTouched();
    });
  }

  isFieldVisible(field: FormField): boolean {
    return !field.hidden;
  }

  isFieldInvalid(fieldKey: string): boolean {
    const control = this.form.get(fieldKey);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getFieldError(fieldKey: string): string {
    const control = this.form.get(fieldKey);
    if (control && control.errors && (control.dirty || control.touched)) {
      const errors = control.errors;
      
      if (errors['required']) return 'This field is required';
      if (errors['minlength']) return `Minimum length is ${errors['minlength'].requiredLength}`;
      if (errors['maxlength']) return `Maximum length is ${errors['maxlength'].requiredLength}`;
      if (errors['min']) return `Minimum value is ${errors['min'].min}`;
      if (errors['max']) return `Maximum value is ${errors['max'].max}`;
      if (errors['email']) return 'Please enter a valid email address';
      if (errors['pattern']) return 'Please enter a valid format';
      
      // Custom error messages
      const errorKeys = Object.keys(errors);
      if (errorKeys.length > 0) {
        return errors[errorKeys[0]].message || 'Invalid value';
      }
    }
    
    return '';
  }

  getGridColumnClass(field: FormField): string {
    if (this.config.layout === 'grid') {
      const columns = this.config.columns || 12;
      const colSpan = field.colSpan || Math.floor(12 / columns);
      return `col-${colSpan}`;
    }
    return '';
  }

  // Template helper methods
  getFormValue(): any {
    return this.form ? this.form.value : {};
  }

  isFormValid(): boolean {
    return this.form ? this.form.valid : false;
  }

  isFormDirty(): boolean {
    return this.form ? this.form.dirty : false;
  }

  // Form event handlers
  onSubmit(): void {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value);
    } else {
      this.markAllFieldsAsTouched();
    }
  }

  onReset(): void {
    this.reset();
  }

  trackByFieldKey(index: number, field: FormField): string {
    return field.key;
  }

  // Template helper methods
  getLayoutClass(): string {
    const layout = this.config.layout || 'vertical';
    let classes = 'dynamic-form ';
    
    switch (layout) {
      case 'horizontal':
        classes += 'layout-horizontal';
        break;
      case 'grid':
        classes += `layout-grid grid-${this.config.columns || 2}`;
        break;
      case 'inline':
        classes += 'layout-inline';
        break;
      default:
        classes += 'layout-vertical';
    }
    
    if (this.config.cssClass) {
      classes += ` ${this.config.cssClass}`;
    }
    
    return classes;
  }

  getFieldWrapperClass(field: FormField): string {
    let classes = 'form-field';
    
    if (field.cssClass) {
      classes += ` ${field.cssClass}`;
    }
    
    if (field.width === 'full') {
      classes += ' field-full-width';
    } else if (field.width === 'half') {
      classes += ' field-half-width';
    }
    
    return classes;
  }

  shouldShowField(field: FormField): boolean {
    return !field.hidden;
  }

  isFieldRequired(field: FormField): boolean {
    return field.required || (field.validation?.required === true);
  }

  getFieldErrors(fieldKey: string): string[] {
    const control = this.form.get(fieldKey);
    const errors: string[] = [];
    
    if (control && control.invalid && (control.dirty || control.touched)) {
      const fieldErrors = control.errors;
      if (fieldErrors) {
        Object.keys(fieldErrors).forEach(errorKey => {
          switch (errorKey) {
            case 'required':
              errors.push('This field is required');
              break;
            case 'minlength':
              errors.push(`Minimum length is ${fieldErrors[errorKey].requiredLength}`);
              break;
            case 'maxlength':
              errors.push(`Maximum length is ${fieldErrors[errorKey].requiredLength}`);
              break;
            case 'min':
              errors.push(`Minimum value is ${fieldErrors[errorKey].min}`);
              break;
            case 'max':
              errors.push(`Maximum value is ${fieldErrors[errorKey].max}`);
              break;
            case 'email':
              errors.push('Please enter a valid email address');
              break;
            case 'pattern':
              errors.push('Invalid format');
              break;
            default:
              errors.push('Invalid value');
          }
        });
      }
    }
    
    return errors;
  }

  getActionButtonsClass(): string {
    const alignment = this.config.actionButtonAlignment || 'right';
    return `form-actions actions-${alignment}`;
  }
}