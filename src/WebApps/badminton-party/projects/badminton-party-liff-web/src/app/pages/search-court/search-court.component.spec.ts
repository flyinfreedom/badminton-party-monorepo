import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchCourtComponent } from './search-court.component';

describe('SearchCourtComponent', () => {
  let component: SearchCourtComponent;
  let fixture: ComponentFixture<SearchCourtComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SearchCourtComponent]
    });
    fixture = TestBed.createComponent(SearchCourtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
