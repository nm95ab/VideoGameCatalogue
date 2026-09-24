import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, it, expect, beforeEach } from 'vitest';
import { App } from './app';

describe('App Component', () => {
  let component: App;
  let fixture: ComponentFixture<App>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    component = fixture.componentInstance;
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should render Nikola Solutions and logo in the footer', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('footer')?.textContent).toContain('Nikola Solutions');
    const img = compiled.querySelector('footer img');
    expect(img).toBeTruthy();
    expect(img?.getAttribute('src')).toBe('nikola-solutions-logo.jpg');
    expect(img?.getAttribute('alt')).toBe('Nikola Solutions Logo');
  });
});
