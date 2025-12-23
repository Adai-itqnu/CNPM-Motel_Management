import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RecentActivity {
  type: string;
  userName: string;
  description: string;
  amount?: string;
  time: string;
  avatarUrl?: string;
  badge?: {
    text: string;
    color: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ActivityService {
  private apiUrl = 'http://localhost:5001/api/statistics';

  constructor(private http: HttpClient) {}

  getRecent(limit: number = 5): Observable<RecentActivity[]> {
    return this.http.get<RecentActivity[]>(`${this.apiUrl}/recent-activities?limit=${limit}`);
  }
}
