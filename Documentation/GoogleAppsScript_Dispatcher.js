/**
 * GOOGLE APPS SCRIPT: DISPATCHER EM TEMPO REAL PARA UIPATH ORCHESTRATOR
 * 
 * Este script deve ser colado no Editor de Scripts da planilha Google Sheets vinculada ao formulário.
 * Ele intercepta cada novo envio de formulário e adiciona o item diretamente na fila do Orchestrator
 * em menos de 1 segundo, sem a necessidade de rodar um robô Dispatcher em lote.
 */

// ==========================================
// 1. CONFIGURAÇÕES DA API DO UIPATH ORCHESTRATOR
// ==========================================
const CLIENT_ID = "SEU_CLIENT_ID_AQUI";           // Gerado no Automation Cloud (Admin > External Applications)
const CLIENT_SECRET = "SEU_CLIENT_SECRET_AQUI";   // Gerado no Automation Cloud
const ORG_NAME = "SUA_ORGANIZACAO_AQUI";         // Nome da sua Organização na URL do Cloud (ex: "minhaempresa")
const TENANT_NAME = "Default";                    // Nome da Tenant (geralmente "Default")
const FOLDER_ID = "1234567";                      // ID numérico da Folder do Orchestrator (ver na URL do Orchestrator, ex: &fid=1234567)
const QUEUE_NAME = "Pharma_Prescriptions_Queue";  // Nome da fila do Orchestrator

// ==========================================
// 2. FUNÇÃO DISPARADA NO ENVIO DO FORMULÁRIO
// ==========================================
function onFormSubmit(e) {
  try {
    // Coleta os dados do evento de envio do Google Forms
    // e.values contém uma lista das respostas na ordem das colunas da planilha
    var values = e.values;
    
    var timestamp = values[0];         // Coluna A (Timestamp) - Usado como Reference na fila
    var prescriptionUrl = values[1];   // Coluna B (Link da Receita Médica)
    var shippingType = values[2];      // Coluna C (Tipo de Envio / Transportadora)
    
    Logger.log("Novo formulário enviado: Ref=" + timestamp + ", Url=" + prescriptionUrl);
    
    // 1. Obter Token de Acesso OAuth 2.0
    var accessToken = getUiPathToken();
    if (!accessToken) {
      throw new Error("Não foi possível gerar o token de acesso do UiPath.");
    }
    
    // 2. Inserir Item na Fila do Orchestrator
    var success = addQueueItem(accessToken, timestamp, prescriptionUrl, shippingType);
    if (success) {
      Logger.log("Item inserido na fila do Orchestrator com sucesso!");
    }
    
  } catch (error) {
    Logger.log("ERRO NO DISPATCHER: " + error.toString());
  }
}

// ==========================================
// 3. FUNÇÃO PARA AUTENTICAÇÃO OAUTH 2.0 (CLIENT CREDENTIALS)
// ==========================================
function getUiPathToken() {
  var url = "https://cloud.uipath.com/identity_/connect/token";
  
  var payload = {
    "grant_type": "client_credentials",
    "client_id": CLIENT_ID,
    "client_secret": CLIENT_SECRET,
    "scope": "OR.Queues"
  };
  
  var options = {
    "method": "post",
    "contentType": "application/x-www-form-urlencoded",
    "payload": payload,
    "muteHttpExceptions": true
  };
  
  var response = UrlFetchApp.fetch(url, options);
  var json = JSON.parse(response.getContentText());
  
  if (response.getResponseCode() == 200) {
    return json.access_token;
  } else {
    Logger.log("Erro na Autenticação UiPath: " + response.getContentText());
    return null;
  }
}

// ==========================================
// 4. FUNÇÃO PARA INSERIR O ITEM NA FILA
// ==========================================
function addQueueItem(accessToken, timestamp, prescriptionUrl, shippingType) {
  // URL da API OData para adicionar itens de fila
  var url = "https://cloud.uipath.com/" + ORG_NAME + "/" + TENANT_NAME + "/orchestrator_/odata/QueueItems/UiPath.Server.Configuration.OData.AddQueueItem";
  
  var body = {
    "itemData": {
      "Name": QUEUE_NAME,
      "Reference": timestamp.trim(),
      "Priority": "Normal",
      "SpecificContent": {
        "PrescriptionUrl": prescriptionUrl.trim(),
        "ShippingType": shippingType.trim()
      }
    }
  };
  
  var headers = {
    "Authorization": "Bearer " + accessToken,
    "X-UIPATH-OrganizationUnitId": FOLDER_ID, // Define a Folder do Orchestrator
    "Content-Type": "application/json"
  };
  
  var options = {
    "method": "post",
    "headers": headers,
    "payload": JSON.stringify(body),
    "muteHttpExceptions": true
  };
  
  var response = UrlFetchApp.fetch(url, options);
  
  if (response.getResponseCode() == 201 || response.getResponseCode() == 200) {
    return true;
  } else {
    Logger.log("Erro ao adicionar item na fila: " + response.getContentText());
    return false;
  }
}
