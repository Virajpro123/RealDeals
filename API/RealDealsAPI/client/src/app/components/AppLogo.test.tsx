import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { AppLogo } from './AppLogo'; 

test('renders AppLogo component', () => {
  render(<AppLogo />);

  expect(screen.getByTestId('app-logo-icon')).toBeInTheDocument();

  expect(screen.getByTestId('app-logo-image')).toBeInTheDocument();

  const imgElement = screen.getByTestId('app-logo-image') as HTMLImageElement;
  expect(imgElement.src).toContain('Logo.svg');
});