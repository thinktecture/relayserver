import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {
  CheckboxCustomEvent,
  DatetimeCustomEvent,
  IonCheckbox,
  IonDatetime,
  IonDatetimeButton,
  IonItem,
  IonModal,
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-date-input',
  standalone: true,
  imports: [IonItem, IonCheckbox, IonDatetimeButton, IonModal, IonDatetime],
  templateUrl: './date-input.component.html',
  styleUrl: './date-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: DateInputComponent,
      multi: true,
    },
  ],
})
export class DateInputComponent implements ControlValueAccessor {
  label = input.required<string>();

  datetimeId = `datetime-${Math.random()}`;
  onChange: (value: string | null) => void = () => {};
  onTouched = (): void => {};
  disabled = signal(false);
  value = signal<string>(new Date().toISOString().substring(0, 10));
  indeterminate = signal(true);

  constructor() {
    effect(() => {
      const value = this.value();
      this.onChange(this.indeterminate() ? null : this.valueToIsoString(value));
    });
  }

  onCheckboxChange(event: CheckboxCustomEvent): void {
    this.indeterminate.set(!event.detail.checked);
  }

  onDateChange(event: DatetimeCustomEvent): void {
    this.value.set(this.normalizeValue(event.detail.value as string));
  }

  writeValue(value: string | null): void {
    this.indeterminate.set(value === null);
    if (value === null) {
      return;
    }

    this.value.set(this.isoStringToValue(value));
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(disabled: boolean): void {
    this.disabled.set(disabled);
  }

  private normalizeValue(datetime: string): string {
    const date = datetime.substring(0, 10);
    return `${date}T23:59:59`;
  }

  private isoStringToValue(datetime: string): string {
    const date = new Date(datetime);
    const year = date.getFullYear().toString().padStart(4, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return this.normalizeValue(`${year}-${month}-${day}`);
  }

  private valueToIsoString(datetime: string): string {
    return new Date(datetime).toISOString();
  }
}
