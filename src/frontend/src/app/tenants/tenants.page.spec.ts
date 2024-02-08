import { ComponentFixture, TestBed } from '@angular/core/testing'

import { TenantsPage } from './tenants.page'

describe('TenantsPage', () => {
  let component: TenantsPage
  let fixture: ComponentFixture<TenantsPage>

  beforeEach(async () => {
    fixture = TestBed.createComponent(TenantsPage)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
