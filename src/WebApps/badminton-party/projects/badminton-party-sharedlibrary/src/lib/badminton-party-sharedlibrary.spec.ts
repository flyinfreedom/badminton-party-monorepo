import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BadmintonPartySharedlibrary } from './badminton-party-sharedlibrary';

describe('BadmintonPartySharedlibrary', () => {
  let component: BadmintonPartySharedlibrary;
  let fixture: ComponentFixture<BadmintonPartySharedlibrary>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BadmintonPartySharedlibrary],
    }).compileComponents();

    fixture = TestBed.createComponent(BadmintonPartySharedlibrary);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
