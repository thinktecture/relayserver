import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IntervalInputComponent } from './interval-input.component';

describe('IntervalInputComponent', () => {
  let component: IntervalInputComponent;
  let fixture: ComponentFixture<IntervalInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IntervalInputComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(IntervalInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
