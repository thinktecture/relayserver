import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TenantDetailsPage } from './tenant-details.page';

describe('TenantDetailsPage', () => {
  let component: TenantDetailsPage;
  let fixture: ComponentFixture<TenantDetailsPage>;

  beforeEach(async(() => {
    fixture = TestBed.createComponent(TenantDetailsPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
