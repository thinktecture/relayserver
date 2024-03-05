import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TenantConnectionsPage } from './tenant-connections.page';

describe('ConnectionsPage', () => {
  let component: TenantConnectionsPage;
  let fixture: ComponentFixture<TenantConnectionsPage>;

  beforeEach(async () => {
    fixture = TestBed.createComponent(TenantConnectionsPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
