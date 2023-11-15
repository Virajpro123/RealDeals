import { render, screen } from '@testing-library/react';
import LoadingComponent from './LoadingComponent';
import '@testing-library/jest-dom';

describe('LoadingComponent', () => {
  test('renders with default message and visible backdrop', () => {
    render(<LoadingComponent isBackDropInvisible={false} />);

    expect(screen.getByText('Loading...')).toBeInTheDocument();

    expect(screen.getByTestId('backdrop')).toBeVisible();
  });

});