import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class PasswordService {
  constructor(private http: HttpService) {}

  changePassword(data: { currentPassword: string; newPassword: string }): Observable<any> {
    return this.http.post('password/change', data);
  }
}
