import { LinquaWebPage } from './app.po';

describe('linqua-web App', function() {
  let page: LinquaWebPage;

  beforeEach(() => {
    page = new LinquaWebPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
