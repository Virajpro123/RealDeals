import { Box, Button, Typography } from "@mui/material";
import RealDeaLsLogo from "../../assets/HomePageLogo.png"
import { router } from "../../app/router/Routes";

export default function HomePage() {
    
    const style = {
        display: 'flex',
        width: '100%',
        transform: 'translate(0%, -10%)',
    };


    return (
        <>
            <Box
                component="img"
                sx={style} 
                alt="The house from the offer."
                src={RealDeaLsLogo}
            />
             <Typography variant='h1' justifyContent='center' style = {{display: 'flex', width: '100%',transform: 'translate(0%, -150%)', fontWeight: 'bold'}}>
                    Welcome..!
                </Typography>
            <Box display='flex' justifyContent='center' sx={{ p: 4 }} >
            <Button onClick={() => {router.navigate('/Catalog')}} style={{color: '#FFFFFF', background : '#1A2C50',transform: 'translate(0%, -380%)'} }  size='large'  variant="contained" >Go to Movie Catalog</Button>
            </Box>

        </>
    )
}