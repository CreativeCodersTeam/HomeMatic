import {Component, Input} from '@angular/core';
import {BreakpointObserver, Breakpoints} from '@angular/cdk/layout';
import {Router} from '@angular/router';

@Component({
  selector: 'tofu-app-layout',
  standalone: false,
  templateUrl: './tofu-app-layout.component.html',
  styleUrl: './tofu-app-layout.component.scss'
})
export class TofuAppLayoutComponent {
  @Input()
  appTitle = 'the-frontend';

  isSidenavOpen = true;

  sidenavMode: 'over' | 'side' = 'side';

  constructor(private breakpointObserver: BreakpointObserver, private router: Router) {
  }

  ngOnInit(): void {
    this.breakpointObserver.observe([Breakpoints.Handset])
      .subscribe(result => {
        this.sidenavMode = result.matches ? 'over' : 'side';
        this.isSidenavOpen = !result.matches;
      });
  }

  toggleSidenav(sidenav: any) {
    sidenav.toggle();
  }

  navigateTo(route: string) {
    //this.router.navigate([route]);

    if (this.sidenavMode === 'over') {
      this.isSidenavOpen = false;
    }
  }
}
