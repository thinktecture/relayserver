import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TenantStatisticsPage } from './tenant-statistics.page';

describe('TenantStatisticsPage', () => {
  let component: TenantStatisticsPage;
  let fixture: ComponentFixture<TenantStatisticsPage>;

  beforeEach(async(() => {
    fixture = TestBed.createComponent(TenantStatisticsPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
