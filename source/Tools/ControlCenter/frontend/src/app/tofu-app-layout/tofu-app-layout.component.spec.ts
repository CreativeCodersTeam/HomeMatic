import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TofuAppLayoutComponent } from './tofu-app-layout.component';

describe('TofuAppLayoutComponent', () => {
  let component: TofuAppLayoutComponent;
  let fixture: ComponentFixture<TofuAppLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TofuAppLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TofuAppLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
