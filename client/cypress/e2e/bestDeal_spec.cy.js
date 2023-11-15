describe('bestDeal', () =>{
    it('user can get bestDeal Movie', () => {
        //Go to movie list
        cy.visit('/')
        cy.findByTestId('goToCatalog').first().click();
        //Select Get best deal
        cy.findByTestId('getDealButton').first().click(); 
    })
})