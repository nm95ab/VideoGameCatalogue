import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgbCollapseModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  isNavbarCollapsed = true;
}
