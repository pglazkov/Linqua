import { Component } from '@angular/core';
import { Auth } from './auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass'],
  providers: [ Auth ]
})
export class AppComponent {
  title = 'app works!';
  
  constructor(private auth: Auth) {    
  }
  
}
