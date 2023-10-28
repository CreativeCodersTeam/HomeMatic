import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCcuComponent } from './add-ccu.component';

describe('AddCcuComponent', () => {
  let component: AddCcuComponent;
  let fixture: ComponentFixture<AddCcuComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AddCcuComponent]
    });
    fixture = TestBed.createComponent(AddCcuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
