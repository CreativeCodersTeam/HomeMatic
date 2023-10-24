import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CcuListOverviewComponent } from './ccu-list-overview.component';

describe('CcuListOverviewComponent', () => {
  let component: CcuListOverviewComponent;
  let fixture: ComponentFixture<CcuListOverviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CcuListOverviewComponent]
    });
    fixture = TestBed.createComponent(CcuListOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
