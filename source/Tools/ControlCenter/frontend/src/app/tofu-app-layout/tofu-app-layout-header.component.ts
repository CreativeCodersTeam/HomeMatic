import {Component, Input} from '@angular/core';

@Component({
  selector: 'hcf-tofu-app-layout-header',
  standalone: false,
  templateUrl: './tofu-app-layout-header.component.html',
  styleUrl: './tofu-app-layout-header.component.scss'
})
export class TofuAppLayoutHeaderComponent {
  @Input()
  appTitle = '';
}
