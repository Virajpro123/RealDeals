import { AppBar, IconButton, List, ListItem, Toolbar } from "@mui/material";
import { NavLink } from "react-router-dom";
import { AppLogo } from "../components/AppLogo";

const midLinks = [
    { title: 'Movies', path: '/catalog' },
    { title: 'About', path: '/about' },
    { title: 'Contact', path: '/Contact' }
]

const navStyles = {
    color: 'inherit',
    textDecoration: 'none',
    typography: 'h6',
    '&:hover': {
        color: '#D62828'
    },
    '&.active': {
        color: 'text.secondary'
    }
}

export default function Header() {
    return (
        <AppBar position='static'>
            <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <IconButton component={NavLink} to='/' size='large' color='inherit' sx={{ mr: 5, ml: 3 }}>
                    <AppLogo />
                </IconButton>
                <List sx={{ display: 'flex' }}>
                    {midLinks.map(({ title, path }) => (
                        <ListItem
                            component={NavLink}
                            to={path}
                            key={path}
                            sx={navStyles}
                        >
                            {title.toUpperCase()}
                        </ListItem>
                    ))}
                </List>
            </Toolbar>
        </AppBar>
    )
}