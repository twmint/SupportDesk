import { Service, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';

export function formatPageTitle(pageTitle?: string): string {
  return pageTitle ? `SD: ${pageTitle}` : 'Support Desk';
}

@Service()
export class PageTitleStrategy extends TitleStrategy {
  private title = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot) {
    this.title.setTitle(formatPageTitle(this.buildTitle(snapshot)));
  }
}
