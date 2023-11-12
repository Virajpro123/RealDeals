import { Container, CssBaseline } from "@mui/material";
import { createTheme, ThemeProvider } from "@mui/material/styles";
import { Outlet } from "react-router-dom";
import Header from "./Header";


function App() {
  
  const theme = createTheme({
    palette: {
      primary: {
        main: '#EAE2B7',
        dark: '#1A2C50'
      },
      secondary: {
        main: '#D62828',
      },
    }
  })

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Header />
      <Container sx={{ mt: 5 }}>
        <Outlet />
      </Container>
    </ThemeProvider>
  );
}

export default App;
