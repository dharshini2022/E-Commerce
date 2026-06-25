import { BehaviorSubject } from 'rxjs';

export const usernameSubject = new BehaviorSubject<string | undefined>(undefined);

export function getCurrentUserName(): string | undefined {
  return usernameSubject.value;
}
