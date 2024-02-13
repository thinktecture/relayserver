import {
  ChangeDetectionStrategy,
  Component,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { IonInput, InputCustomEvent } from '@ionic/angular/standalone';

@Component({
  selector: 'app-interval-input',
  standalone: true,
  imports: [IonInput],
  templateUrl: './interval-input.component.html',
  styleUrl: './interval-input.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: IntervalInputComponent,
      multi: true,
    },
  ],
})
export class IntervalInputComponent implements ControlValueAccessor {
  label = input<string>();
  placeholder = input<string>();
  helperText = input<string>();
  errorText = input<string>();

  onChange: (event: InputCustomEvent) => void = () => {};
  onTouched = () => {};
  disabled = signal(false);
  value = signal<number | null>(null);

  writeValue(value: string | null): void {
    console.log(`IIC: value=${JSON.stringify(value)}`);
    this.value.set(this.intervalToSecons(value));
  }

  registerOnChange(fn: (interval: string | null) => void): void {
    const fn2 = (interval: string | null) => {
      console.log(`IIC#onChange: value=${JSON.stringify(interval)}`);
      fn(interval);
    };

    this.onChange = (interval: InputCustomEvent) => {
      fn2(this.secondsToInterval(interval.detail.value));
    };
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(disabled: boolean): void {
    this.disabled.set(disabled);
  }

  private secondsToInterval(
    totalSecondsStr: string | null | undefined,
  ): string | null {
    if (
      totalSecondsStr === null ||
      totalSecondsStr === undefined ||
      totalSecondsStr.length == 0
    ) {
      return null;
    }

    const totalSeconds = Number(totalSecondsStr);
    const totalMinutes = Math.trunc(totalSeconds / 60);
    const totalHours = Math.trunc(totalMinutes / 60);
    const totalDays = Math.trunc(totalHours / 24);

    const format = Intl.NumberFormat(undefined, { minimumIntegerDigits: 2 });
    const seconds = format.format(totalSeconds % 60);
    const minutes = format.format(totalMinutes % 60);
    const hours = format.format(totalHours % 24);
    const days = totalDays.toString();

    const interval = `${hours}:${minutes}:${seconds}`;
    if (totalDays === 0) {
      return interval;
    }

    return `${days}.${interval}`;
  }

  private intervalToSecons(interval: string | null): number | null {
    if (interval === null) {
      return null;
    }

    const [daysAndHours, minutes, seconds] = interval.split(':');
    let days = '0';
    let hours = daysAndHours;
    if (hours.includes('.')) {
      [days, hours] = hours.split('.');
    }

    return (
      ((Number(days) * 24 + Number(hours)) * 60 + Number(minutes)) * 60 +
      Number(seconds)
    );
  }
}
