import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TofuAppLayoutHeaderComponent } from './tofu-app-layout-header.component';

describe('TofuAppLayoutHeaderComponent', () => {
  let component: TofuAppLayoutHeaderComponent;
  let fixture: ComponentFixture<TofuAppLayoutHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TofuAppLayoutHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TofuAppLayoutHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
