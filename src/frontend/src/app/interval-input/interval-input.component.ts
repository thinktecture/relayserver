import {
  ChangeDetectionStrategy,
  Component,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { IonInput, InputCustomEvent, IonItem } from '@ionic/angular/standalone';

@Component({
  selector: 'app-interval-input',
  standalone: true,
  imports: [IonItem, IonInput],
  templateUrl: './interval-input.component.html',
  styleUrl: './interval-input.component.scss',
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
  onTouched = (): void => {};
  disabled = signal(false);
  value = signal<number | null>(null);

  writeValue(value: string | null): void {
    this.value.set(this.intervalToSeconds(value));
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = (event: InputCustomEvent) => {
      fn(this.secondsToInterval(event.detail.value));
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

    const totalSeconds = Math.abs(Number(totalSecondsStr));
    if (Number.isNaN(totalSeconds)) {
      return null;
    }

    const totalMinutes = Math.trunc(totalSeconds / 60);
    const totalHours = Math.trunc(totalMinutes / 60);
    const totalDays = Math.trunc(totalHours / 24);

    const format = Intl.NumberFormat(undefined, { minimumIntegerDigits: 2 });
    const seconds = format.format(totalSeconds % 60);
    const minutes = format.format(totalMinutes % 60);
    const hours = format.format(totalHours % 24);
    const days = totalDays.toString();

    const interval = `${hours}:${minutes}:${seconds}`;
    const sign = Number(totalSecondsStr) < 0 ? '-' : '';
    if (totalDays === 0) {
      return `${sign}${interval}`;
    }

    return `${sign}${days}.${interval}`;
  }

  private intervalToSeconds(interval: string | null): number | null {
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
