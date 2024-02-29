import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConnectionsPage } from './connections.page';

describe('ConnectionsPage', () => {
  let component: ConnectionsPage;
  let fixture: ComponentFixture<ConnectionsPage>;

  beforeEach(async(() => {
    fixture = TestBed.createComponent(ConnectionsPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
